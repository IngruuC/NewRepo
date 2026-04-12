using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class ProveedorController : Controller
    {
        private readonly ControladoraProveedor _controladora;
        private readonly ControladoraProducto _controladoraProducto;
        private readonly ControladoraCompra _controladoraCompra;
        private readonly ControladoraUsuario _controladoraUsuario;
        private readonly GestorRelacionProveedorProducto _gestor;


        public ProveedorController()
        {
            _controladora = ControladoraProveedor.ObtenerInstancia();
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
            _controladoraCompra = ControladoraCompra.ObtenerInstancia();
            _controladoraUsuario = ControladoraUsuario.ObtenerInstancia();
            _gestor = GestorRelacionProveedorProducto.ObtenerInstancia();

        }

        private Proveedor ObtenerProveedor()
        {
            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            return _controladora.ObtenerProveedores().FirstOrDefault(p => p.UsuarioId == usuarioId);
        }

        private void SetLayoutData(Proveedor proveedor = null)
        {
            proveedor ??= ObtenerProveedor();
            ViewBag.ProveedorNombre = proveedor?.RazonSocial ?? SessionHelper.GetUsuarioNombre(HttpContext.Session);
            ViewBag.Proveedor = proveedor;
        }

        private bool EsProveedor() =>
            SessionHelper.EsProveedor(HttpContext.Session) || SessionHelper.EsAdministrador(HttpContext.Session);

        public IActionResult Index()
        {
            if (!EsProveedor()) return RedirectToAction("AccessDenied", "Account");
            var proveedor = ObtenerProveedor();
            if (proveedor == null) return RedirectToAction("Logout", "Account");
            SetLayoutData(proveedor);
            var compras = _controladoraCompra.ObtenerCompras()
                .Where(c => c.ProveedorId == proveedor.Id)
                .OrderByDescending(c => c.FechaCompra)
                .ToList();
            ViewBag.MisCompras = compras;
            var todosProductos = _controladoraProducto.ObtenerProductos();
            var misProductos = _gestor.ObtenerProductosDeProveedor(proveedor.Id, todosProductos);
            ViewBag.MisProductos = misProductos;
            var catalogo = ControladoraCatalogo.ObtenerInstancia().ObtenerPorProveedor(proveedor.Id);
            ViewBag.MiCatalogo = catalogo;
            if (TempData.ContainsKey("ActivePanel"))
                ViewBag.ActivePanel = TempData["ActivePanel"];
            return View();
        }

        [HttpPost]
        public IActionResult GuardarPerfil(Proveedor proveedor)
        {
            try
            {
                var actual = ObtenerProveedor();
                if (actual == null) throw new Exception("Proveedor no encontrado.");
                actual.RazonSocial = proveedor.RazonSocial;
                actual.Telefono = proveedor.Telefono;
                actual.Email = proveedor.Email;
                actual.Direccion = proveedor.Direccion;
                _controladora.ModificarProveedor(actual);
                TempData["Success"] = "Perfil actualizado correctamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = "perfil";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CambiarContrasena(string contrasenaActual, string nuevaContrasena, string confirmarContrasena)
        {
            try
            {
                if (nuevaContrasena != confirmarContrasena)
                    throw new Exception("Las contraseñas no coinciden.");
                var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session).Value;
                var resultado = _controladoraUsuario.ValidarCredenciales(
                    SessionHelper.GetUsuarioNombre(HttpContext.Session), contrasenaActual);
                if (!resultado.Exitoso) throw new Exception("Contraseña actual incorrecta.");
                _controladoraUsuario.CambiarContrasena(usuarioId, nuevaContrasena);
                TempData["Success"] = "Contraseña actualizada.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = "perfil";
            return RedirectToAction("Index");
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult Crear(Proveedor proveedor, string __panel)
        {
            try
            {
                _controladora.AgregarProveedor(proveedor);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Nuevo Proveedor", $"{proveedor.RazonSocial} — CUIT: {proveedor.Cuit}", ip);

                TempData["Success"] = "Proveedor creado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "proveedores";
            return RedirectToAction("Index", "Admin");
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult Editar(Proveedor proveedor, string __panel)
        {
            try
            {
                _controladora.ModificarProveedor(proveedor);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Modificó Proveedor", $"{proveedor.RazonSocial} — ID: {proveedor.Id}", ip);

                TempData["Success"] = "Proveedor actualizado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "proveedores";
            return RedirectToAction("Index", "Admin");
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult Eliminar(int id, string __panel)
        {
            try
            {
                _controladora.EliminarProveedor(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Eliminó Proveedor", $"Proveedor ID: {id}", ip);

                TempData["Success"] = "Proveedor eliminado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "proveedores";
            return RedirectToAction("Index", "Admin");
        }

        [AdminOnlyFilter]
        [HttpGet]
        public JsonResult ObtenerProductosProveedor(int proveedorId)
        {
            var todos = _controladoraProducto.ObtenerProductos();
            var asignados = _gestor.ObtenerIdsProductosDeProveedor(proveedorId);
            var resultado = todos.Select(p => new {
                id = p.Id,
                nombre = p.Nombre,
                codigo = p.CodigoBarra,
                precio = p.Precio,
                stock = p.Stock,
                asignado = asignados.Contains(p.Id)
            });
            return Json(resultado);
        }

        [AdminOnlyFilter]
        [HttpPost]
        public JsonResult GuardarProductosProveedor(int proveedorId, [FromBody] List<int> productosIds)
        {
            try
            {
                var todos = _controladoraProducto.ObtenerProductos();
                var actuales = _gestor.ObtenerIdsProductosDeProveedor(proveedorId);
                foreach (var id in actuales.ToList())
                    if (!productosIds.Contains(id))
                        _gestor.QuitarProductoDeProveedor(proveedorId, id);
                foreach (var id in productosIds)
                {
                    if (!actuales.Contains(id))
                    {
                        var prod = todos.FirstOrDefault(p => p.Id == id);
                        if (prod != null)
                            _gestor.AsignarProductoAProveedor(proveedorId, prod.Id, prod.Nombre);
                    }
                }
                return Json(new { ok = true, mensaje = "Productos actualizados correctamente." });
            }
            catch (Exception ex) { return Json(new { ok = false, error = ex.Message }); }
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult AsignarUsuarioDefault(int id, string __panel)
        {
            try
            {
                _controladoraUsuario.AsignarUsuarioDefaultAProveedor(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Asignó Usuario Default Proveedor",
                    $"Proveedor ID: {id}", ip);

                TempData["Success"] = "Usuario de proveedor asignado correctamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "proveedores";
            return RedirectToAction("Index", "Admin");
        }
    }
}