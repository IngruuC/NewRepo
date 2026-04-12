using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using PROYECTO_WEB.TESIS.Models;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class VentaController : Controller
    {
        private readonly ControladoraVenta _controladoraVenta;
        private readonly ControladoraCliente _controladoraCliente;
        private readonly ControladoraProducto _controladoraProducto;

        public VentaController()
        {
            _controladoraVenta = ControladoraVenta.ObtenerInstancia();
            _controladoraCliente = ControladoraCliente.ObtenerInstancia();
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult Crear(VentaViewModel model, string __panel)
        {
            try
            {
                var venta = new Venta { ClienteId = model.ClienteId, FormaPago = model.FormaPago, FechaVenta = DateTime.Now };
                foreach (var d in model.Detalles.Where(x => x.Cantidad > 0))
                    venta.Detalles.Add(new DetalleVenta { ProductoId = d.ProductoId, Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario, ProductoNombre = d.ProductoNombre, Subtotal = d.PrecioUnitario * d.Cantidad });
                _controladoraVenta.RealizarVenta(venta);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var total = venta.Detalles.Sum(d => d.Subtotal);
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Nueva Venta", $"Venta — Cliente ID: {model.ClienteId} — Total: ${total:N2}", ip);

                TempData["Success"] = "Venta realizada con éxito.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "nueva-venta";
            return RedirectToAction("Index", "Admin");
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult Eliminar(int id, string __panel)
        {
            try
            {
                _controladoraVenta.EliminarVenta(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Eliminó Venta", $"Venta ID: {id}", ip);

                TempData["Success"] = "Venta eliminada.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "ventas-totales";
            return RedirectToAction("Index", "Admin");
        }

        public IActionResult MisCompras()
        {
            if (!SessionHelper.EsCliente(HttpContext.Session))
                return RedirectToAction("AccessDenied", "Account");
            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            var cliente = _controladoraCliente.ObtenerClientes().FirstOrDefault(c => c.UsuarioId == usuarioId);
            if (cliente == null) return RedirectToAction("Index", "PanelCliente");
            var ventas = _controladoraVenta.ObtenerVentas()
                .Where(v => v.ClienteId == cliente.Id).OrderByDescending(v => v.FechaVenta).ToList();
            return View(ventas);
        }
    }
}