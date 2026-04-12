using ENTIDADES;
using MODELO;

namespace CONTROLADORA
{
    public class ControladoraAuditoria
    {
        private Contexto contexto;

        public ControladoraAuditoria()
        {
            contexto = new Contexto();
        }

        public List<AuditoriaSesion> ObtenerHistorialSesiones(
            int? usuarioId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            try
            {
                var query = contexto.AuditoriasSesion.AsQueryable();

                if (usuarioId.HasValue)
                    query = query.Where(a => a.UsuarioId == usuarioId.Value);

                if (fechaDesde.HasValue)
                    query = query.Where(a => a.FechaIngreso >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(a => a.FechaIngreso <= fechaHasta.Value);

                return query.OrderByDescending(a => a.FechaIngreso).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<AuditoriaSesion>();
            }
        }

        public List<AuditoriaSesion> ObtenerTodasLasSesiones()
        {
            try
            {
                return contexto.AuditoriasSesion
                    .OrderByDescending(a => a.FechaIngreso)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<AuditoriaSesion>();
            }
        }

        public void RegistrarInicioSesion(Usuario usuario, string direccionIP)
        {
            if (usuario == null) return;

            try
            {
                var auditoria = new AuditoriaSesion
                {
                    UsuarioId = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    FechaIngreso = DateTime.Now,
                    DireccionIP = direccionIP,
                    Dispositivo = Environment.MachineName,
                    TipoSesion = usuario.Rol,
                    SesionActiva = true
                };

                contexto.AuditoriasSesion.Add(auditoria);
                contexto.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar inicio de sesión: {ex.Message}");
            }
        }

        public void RegistrarCierreSesion(int usuarioId)
        {
            try
            {
                var sesionActiva = contexto.AuditoriasSesion
                    .Where(a => a.UsuarioId == usuarioId && a.SesionActiva)
                    .OrderByDescending(a => a.FechaIngreso)
                    .FirstOrDefault();

                if (sesionActiva != null)
                {
                    sesionActiva.FechaSalida = DateTime.Now;
                    sesionActiva.SesionActiva = false;
                    contexto.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cerrar sesión: {ex.Message}");
            }
        }
    }
}