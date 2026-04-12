using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Models;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class CompraController : Controller
    {
        private readonly ControladoraCompra _controladoraCompra;
        private readonly ControladoraProducto _controladoraProducto;
        private readonly GestorRelacionProveedorProducto _gestor;

        public CompraController()
        {
            _controladoraCompra = ControladoraCompra.ObtenerInstancia();
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
            _gestor = GestorRelacionProveedorProducto.ObtenerInstancia();
        }

        [HttpGet]
        public JsonResult ObtenerProductosProveedor(int proveedorId)
        {
            var todos = _controladoraProducto.ObtenerProductos();
            var productos = _gestor.ObtenerProductosDeProveedor(proveedorId, todos);
            if (!productos.Any()) productos = todos;
            return Json(productos.Select(p => new { p.Id, p.Nombre, p.Precio, p.Stock }));
        }

        [HttpPost]
        public IActionResult Crear(CompraViewModel model, string __panel)
        {
            try
            {
                var compra = new Compra { ProveedorId = model.ProveedorId, FormaPago = model.FormaPago, NumeroFactura = model.NumeroFactura, FechaCompra = DateTime.Now };
                foreach (var d in model.Detalles.Where(x => x.Cantidad > 0))
                    compra.Detalles.Add(new DetalleCompra { ProductoId = d.ProductoId, Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario, ProductoNombre = d.ProductoNombre, Subtotal = d.PrecioUnitario * d.Cantidad });
                _controladoraCompra.RealizarCompra(compra);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var total = compra.Detalles.Sum(d => d.Subtotal);
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Nueva Compra", $"Compra — Factura: {model.NumeroFactura} — Total: ${total:N2}", ip);

                TempData["Success"] = "Compra realizada con éxito.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "nueva-compra";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id, string __panel)
        {
            try
            {
                _controladoraCompra.EliminarCompra(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Eliminó Compra", $"Compra ID: {id}", ip);

                TempData["Success"] = "Compra eliminada.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "compras-totales";
            return RedirectToAction("Index", "Admin");
        }
    }
}