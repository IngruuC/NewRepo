// Controllers/BackupController.cs — versión PostgreSQL
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class BackupController : Controller
    {
        private readonly string _backupDir;
        private readonly string _connectionString;
        private readonly string _nombreBD;

        public BackupController(IConfiguration config, IWebHostEnvironment env)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Database=railway;Username=postgres;Password=postgres;Port=5432";

            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            _nombreBD = builder.Database ?? "railway";

            _backupDir = Path.Combine(env.ContentRootPath, "Backups");
            if (!Directory.Exists(_backupDir))
                Directory.CreateDirectory(_backupDir);
        }

        // ── Listar backups disponibles ─────────────────────────────────────
        [HttpGet]
        public JsonResult ListarBackups()
        {
            try
            {
                var archivos = Directory.GetFiles(_backupDir, "*.sql")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Select(f => new {
                        nombre = f.Name,
                        fecha = f.CreationTime.ToString("dd/MM/yyyy HH:mm:ss"),
                        tamaño = FormatBytes(f.Length)
                    }).ToList();

                return Json(new { ok = true, backups = archivos });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ── Crear backup ───────────────────────────────────────────────────
        [HttpPost]
        public async Task<JsonResult> CrearBackup()
        {
            try
            {
                var ahora = DateTime.Now;
                var nombreArchivo = $"{ahora:dd-MM-yyyy-HH-mm-ss}-{_nombreBD}.sql";
                var rutaCompleta = Path.Combine(_backupDir, nombreArchivo);

                var sb = new StringBuilder();
                sb.AppendLine($"-- Backup de {_nombreBD} generado el {ahora:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine();

                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // Obtener todas las tablas
                var tablas = new List<string>();
                using (var cmd = new NpgsqlCommand(
                    "SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename",
                    conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        tablas.Add(reader.GetString(0));
                }

                // Para cada tabla, exportar los datos
                foreach (var tabla in tablas)
                {
                    sb.AppendLine($"-- Tabla: {tabla}");
                    sb.AppendLine($"DELETE FROM \"{tabla}\";");

                    using var cmdData = new NpgsqlCommand($"SELECT * FROM \"{tabla}\"", conn);
                    using var readerData = await cmdData.ExecuteReaderAsync();

                    while (await readerData.ReadAsync())
                    {
                        var cols = new List<string>();
                        var vals = new List<string>();

                        for (int i = 0; i < readerData.FieldCount; i++)
                        {
                            cols.Add($"\"{readerData.GetName(i)}\"");
                            var val = readerData.IsDBNull(i) ? "NULL" : $"'{readerData.GetValue(i).ToString()?.Replace("'", "''")}'";
                            vals.Add(val);
                        }

                        sb.AppendLine($"INSERT INTO \"{tabla}\" ({string.Join(", ", cols)}) VALUES ({string.Join(", ", vals)});");
                    }
                    sb.AppendLine();
                }

                await System.IO.File.WriteAllTextAsync(rutaCompleta, sb.ToString());

                return Json(new { ok = true, mensaje = $"Backup creado: {nombreArchivo}", archivo = nombreArchivo });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ── Restaurar backup ───────────────────────────────────────────────
        [HttpPost]
        public async Task<JsonResult> RestaurarBackup(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return Json(new { ok = false, error = "Nombre de archivo requerido." });

            var ruta = Path.Combine(_backupDir, nombreArchivo);
            if (!System.IO.File.Exists(ruta))
                return Json(new { ok = false, error = "El archivo no existe." });

            try
            {
                var sql = await System.IO.File.ReadAllTextAsync(ruta);

                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.CommandTimeout = 300;
                await cmd.ExecuteNonQueryAsync();

                return Json(new { ok = true, mensaje = "Restauración realizada con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ── Descargar backup ───────────────────────────────────────────────
        [HttpGet]
        public IActionResult DescargarBackup(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return BadRequest();

            var nombre = Path.GetFileName(nombreArchivo);
            var ruta = Path.Combine(_backupDir, nombre);

            if (!System.IO.File.Exists(ruta))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(ruta);
            return File(bytes, "application/octet-stream", nombre);
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:N1} MB";
            if (bytes >= 1_024) return $"{bytes / 1_024.0:N1} KB";
            return $"{bytes} B";
        }
    }
}