using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class PanelClienteController : Controller
    {
        private readonly ControladoraProducto _controladoraProducto;
        private readonly ControladoraVenta _controladoraVenta;
        private readonly ControladoraCliente _controladoraCliente;
        private readonly ControladoraUsuario _controladoraUsuario;
        private readonly ControladoraPromocion _controladoraPromocion;

        public PanelClienteController()
        {
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
            _controladoraVenta = ControladoraVenta.ObtenerInstancia();
            _controladoraCliente = ControladoraCliente.ObtenerInstancia();
            _controladoraUsuario = ControladoraUsuario.ObtenerInstancia();
            _controladoraPromocion = ControladoraPromocion.ObtenerInstancia();
        }

        public IActionResult Index()
        {
            if (!SessionHelper.EsCliente(HttpContext.Session))
                return RedirectToAction("AccessDenied", "Account");

            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            var cliente = _controladoraCliente.ObtenerClientes()
                .FirstOrDefault(c => c.UsuarioId == usuarioId);

            if (cliente == null) return RedirectToAction("Logout", "Account");

            var productos = _controladoraProducto.ObtenerProductos()
                .Where(p => p.Stock > 0).ToList();

            var misVentas = _controladoraVenta.ObtenerVentas()
                .Where(v => v.ClienteId == cliente.Id)
                .OrderByDescending(v => v.FechaVenta)
                .ToList();

            ViewBag.Cliente = cliente;
            ViewBag.Productos = productos;
            ViewBag.MisVentas = misVentas;
            ViewBag.NombreCliente = cliente.Nombre;
            ViewBag.PromocionesVigentes = _controladoraPromocion.ObtenerVigentes();

            ViewBag.FavoritosIds = ControladoraFavorito.ObtenerInstancia()
    .ObtenerPorCliente(cliente.Id)
    .Select(f => f.ProductoId)
    .ToList();

            // Restaurar panel activo si viene de un POST
            if (TempData.ContainsKey("ActivePanel"))
                ViewBag.ActivePanel = TempData["ActivePanel"];

            return View();
        }

        [HttpPost]
        public IActionResult RealizarCompra(PROYECTO_WEB.TESIS.Models.VentaViewModel model)
        {
            try
            {
                var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session).Value;
                var cliente = _controladoraCliente.ObtenerClientes()
                    .FirstOrDefault(c => c.UsuarioId == usuarioId);

                if (cliente == null)
                    throw new Exception("Cliente no encontrado.");

                if (model.Detalles == null || !model.Detalles.Any())
                    throw new Exception("El carrito está vacío.");

                var venta = new Venta
                {
                    ClienteId = cliente.Id,
                    FormaPago = model.FormaPago,
                    FechaVenta = DateTime.Now
                };

                foreach (var d in model.Detalles.Where(x => x.Cantidad > 0))
                {
                    venta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        ProductoNombre = d.ProductoNombre,
                        Subtotal = d.PrecioUnitario * d.Cantidad
                    });
                }

                _controladoraVenta.RealizarVenta(venta);
                TempData["Success"] = $"¡Compra realizada con éxito! Total: ${venta.Total:N2}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            TempData["ActivePanel"] = "compras";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Perfil(Cliente cliente)
        {
            try
            {
                _controladoraCliente.ModificarCliente(cliente);
                TempData["Success"] = "Perfil actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            TempData["ActivePanel"] = "perfil";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CambiarContrasena(string contrasenaActual, string nuevaContrasena)
        {
            try
            {
                var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session).Value;
                var nombreUsuario = SessionHelper.GetUsuarioNombre(HttpContext.Session);

                var resultado = _controladoraUsuario.ValidarCredenciales(nombreUsuario, contrasenaActual);
                if (!resultado.Exitoso)
                {
                    TempData["Error"] = "La contraseña actual es incorrecta.";
                    TempData["ActivePanel"] = "perfil";
                    return RedirectToAction("Index");
                }

                _controladoraUsuario.CambiarContrasena(usuarioId, nuevaContrasena);
                TempData["Success"] = "Contraseña actualizada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            TempData["ActivePanel"] = "perfil";
            return RedirectToAction("Index");
        }
    }
}