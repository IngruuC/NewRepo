// Controllers/AdminController.cs
using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class AdminController : Controller
    {
        private readonly ControladoraCliente _controladoraCliente;
        private readonly ControladoraProducto _controladoraProducto;
        private readonly ControladoraVenta _controladoraVenta;
        private readonly ControladoraProveedor _controladoraProveedor;
        private readonly ControladoraCompra _controladoraCompra;
        private readonly ControladoraAuditoria _controladoraAuditoria;
        private readonly ControladoraUsuario _controladoraUsuario;
        private readonly ControladoraSeguridad _controladoraSeguridad;

        public AdminController()
        {
            _controladoraCliente = ControladoraCliente.ObtenerInstancia();
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
            _controladoraVenta = ControladoraVenta.ObtenerInstancia();
            _controladoraProveedor = ControladoraProveedor.ObtenerInstancia();
            _controladoraCompra = ControladoraCompra.ObtenerInstancia();
            _controladoraAuditoria = new ControladoraAuditoria();
            _controladoraUsuario = ControladoraUsuario.ObtenerInstancia();
            _controladoraSeguridad = ControladoraSeguridad.Instancia;
        }

        public IActionResult Index()
        {
            ViewBag.UsuarioNombre = SessionHelper.GetUsuarioNombre(HttpContext.Session);

            ViewBag.Clientes = _controladoraCliente.ObtenerClientes();

            var todosProductos = _controladoraProducto.ObtenerProductos().OrderBy(p => p.Id).ToList();
            ViewBag.TodosProductos = todosProductos;
            ViewBag.Productos = todosProductos.Where(p => p.Stock > 0).ToList();

            ViewBag.Ventas = _controladoraVenta.ObtenerVentas();
            ViewBag.Proveedores = _controladoraProveedor.ObtenerProveedores();
            ViewBag.Compras = _controladoraCompra.ObtenerCompras();

            List<Grupo> grupos;
            try
            {
                using var ctx = new Contexto();
                grupos = ctx.Grupos
                    .Include(g => g.EstadoGrupo)
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR grupos: {ex.Message}");
                grupos = new List<Grupo>();
            }
            ViewBag.Grupos = grupos;
            ViewBag.GruposCount = grupos.Count;

            List<Usuario> usuarios;
            try
            {
                using var ctx2 = new Contexto();
                usuarios = ctx2.Usuarios
                    .Include(u => u.Grupos)
                    .Include(u => u.EstadoUsuario)
                    .AsNoTracking()
                    .ToList();
                foreach (var u in usuarios)
                {
                    u.Email ??= string.Empty;
                    u.NombreyApellido ??= string.Empty;
                    u.Rol ??= string.Empty;
                    u.Grupos ??= new List<Grupo>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR usuarios: {ex.Message}");
                usuarios = new List<Usuario>();
            }
            ViewBag.Usuarios = usuarios;

            ViewBag.Sesiones = _controladoraAuditoria.ObtenerTodasLasSesiones();
            ViewBag.LogActividad = ControladoraLog.ObtenerInstancia().ObtenerRecientes(100);
            ViewBag.EstadosUsuario = _controladoraSeguridad.RecuperarEstadosUsuario();
            ViewBag.EstadosGrupo = _controladoraSeguridad.RecuperarEstadosGrupo();

            ViewBag.CatalogoPendiente = ControladoraCatalogo.ObtenerInstancia().ObtenerPendientes();
            ViewBag.Promociones = ControladoraPromocion.ObtenerInstancia().ObtenerTodas();
            ViewBag.PromocionesVigentes = ControladoraPromocion.ObtenerInstancia().ObtenerVigentes();

            // ── DASHBOARD ──
            var ahora = DateTime.Now;
            var hoy = ahora.Date;

            ViewBag.CantClientes = _controladoraCliente.ObtenerClientes().Count;

            using (var ctxDash = new MODELO.Contexto())
            {
                ViewBag.UsuariosActivos = ctxDash.Usuarios.Count(u => u.Estado);
            }

            ViewBag.OfertasPendientes = ControladoraCatalogo.ObtenerInstancia().ObtenerPendientes().Count;

            var todasVentas = _controladoraVenta.ObtenerVentas();
            var todasCompras = _controladoraCompra.ObtenerCompras();

            ViewBag.VentasHoy = todasVentas.Count(v => v.FechaVenta.Date == hoy);
            ViewBag.ComprasHoy = todasCompras.Count(c => c.FechaCompra.Date == hoy);

            // Estadísticas semana actual vs semana pasada
            var inicioSemanaActual = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioSemanaPasada = inicioSemanaActual.AddDays(-7);
            var finSemanaPasada = inicioSemanaActual.AddDays(-1);

            ViewBag.VentasSemanaActual = todasVentas.Count(v => v.FechaVenta.Date >= inicioSemanaActual);
            ViewBag.VentasSemanaPasada = todasVentas.Count(v => v.FechaVenta.Date >= inicioSemanaPasada && v.FechaVenta.Date <= finSemanaPasada);
            ViewBag.ComprasSemanaActual = todasCompras.Count(c => c.FechaCompra.Date >= inicioSemanaActual);
            ViewBag.ComprasSemanaPasada = todasCompras.Count(c => c.FechaCompra.Date >= inicioSemanaPasada && c.FechaCompra.Date <= finSemanaPasada);

            // Actividad reciente
            var actividad = new List<(DateTime Fecha, string Icono, string Descripcion, string Color)>();
            foreach (var v in todasVentas.OrderByDescending(v => v.FechaVenta).Take(5))
                actividad.Add((v.FechaVenta, "venta", $"Venta #{v.Id} — {v.Cliente?.ToString() ?? "Cliente"} — ${v.Total:N2}", "#27ae60"));
            foreach (var c in todasCompras.OrderByDescending(c => c.FechaCompra).Take(3))
                actividad.Add((c.FechaCompra, "compra", $"Compra #{c.Id} — {c.Proveedor?.RazonSocial ?? "Proveedor"} — ${c.Total:N2}", "#2980b9"));
            ViewBag.ActividadReciente = actividad.OrderByDescending(a => a.Fecha).Take(6).ToList();

            // Productos perecederos próximos a vencer (7 días)
            var en7dias = hoy.AddDays(7);
            ViewBag.ProductosProximosVencer = _controladoraProducto.ObtenerProductos()
                .Where(p => p.EsPerecedero && p.FechaVencimiento.HasValue
                         && p.FechaVencimiento.Value.Date >= hoy
                         && p.FechaVencimiento.Value.Date <= en7dias)
                .OrderBy(p => p.FechaVencimiento)
                .ToList();

            // Último backup
            ViewBag.UltimoBackup = "—";
            try
            {
                var backupPath = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
                if (Directory.Exists(backupPath))
                {
                    var ultimo = Directory.GetFiles(backupPath, "*.bak")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.CreationTime)
                        .FirstOrDefault();
                    if (ultimo != null)
                        ViewBag.UltimoBackup = ultimo.CreationTime.ToString("dd/MM/yyyy HH:mm");
                }
            }
            catch { }

            return View();
        }
    }
}