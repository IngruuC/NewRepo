using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class CatalogoController : Controller
    {
        private readonly ControladoraCatalogo _controladora;
        private readonly ControladoraProducto _controladoraProducto;
        private readonly ControladoraProveedor _controladoraProveedor;
        private readonly ControladoraCompra _controladoraCompra;
        private readonly GestorRelacionProveedorProducto _gestor;

        public CatalogoController()
        {
            _controladora = ControladoraCatalogo.ObtenerInstancia();
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
            _controladoraProveedor = ControladoraProveedor.ObtenerInstancia();
            _controladoraCompra = ControladoraCompra.ObtenerInstancia();
            _gestor = GestorRelacionProveedorProducto.ObtenerInstancia();
        }

        private int? ObtenerProveedorId()
        {
            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            if (usuarioId == null) return null;
            var proveedor = _controladoraProveedor.ObtenerProveedores()
                .FirstOrDefault(p => p.UsuarioId == usuarioId);
            return proveedor?.Id;
        }

        [HttpPost]
        public IActionResult Agregar(CatalogoProveedor item)
        {
            try
            {
                var proveedorId = ObtenerProveedorId();
                if (proveedorId == null) throw new Exception("Proveedor no encontrado.");
                item.ProveedorId = proveedorId.Value;
                _controladora.Agregar(item);
                TempData["Success"] = "Producto agregado al catálogo.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = "catalogo";
            return RedirectToAction("Index", "Proveedor");
        }

        [HttpPost]
        public IActionResult Modificar(CatalogoProveedor item)
        {
            try { _controladora.Modificar(item); TempData["Success"] = "Producto actualizado."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = "catalogo";
            return RedirectToAction("Index", "Proveedor");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try { _controladora.Eliminar(id); TempData["Success"] = "Producto eliminado del catálogo."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = "catalogo";
            return RedirectToAction("Index", "Proveedor");
        }

        [AdminOnlyFilter]
        [HttpGet]
        public JsonResult ObtenerCatalogoProveedor(int proveedorId)
        {
            var items = _controladora.ObtenerPorProveedor(proveedorId);
            return Json(items.Select(i => new {
                i.Id,
                i.NombreProducto,
                i.Precio,
                i.Descripcion,
                i.Estado,
                fecha = i.FechaOferta.ToString("dd/MM/yyyy")
            }));
        }

        [AdminOnlyFilter]
        [HttpGet]
        public JsonResult ObtenerInfoAprobacion(int id)
        {
            using var ctx = new MODELO.Contexto();
            var item = ctx.CatalogoProveedores.Include(c => c.Proveedor).FirstOrDefault(c => c.Id == id);
            if (item == null) return Json(new { ok = false });
            return Json(new
            {
                ok = true,
                id = item.Id,
                nombreProducto = item.NombreProducto,
                precio = item.Precio,
                descripcion = item.Descripcion,
                proveedorId = item.ProveedorId,
                proveedorNombre = item.Proveedor?.RazonSocial ?? "—"
            });
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult AprobarConCompra(int id, int cantidad, decimal precioUnitario, string numeroFactura, string __panel)
        {
            try
            {
                using var ctx = new MODELO.Contexto();
                var item = ctx.CatalogoProveedores.Include(c => c.Proveedor).FirstOrDefault(c => c.Id == id);
                if (item == null) throw new Exception("Oferta no encontrada.");

                var producto = new Producto
                {
                    Nombre = item.NombreProducto,
                    CodigoBarra = new Random().Next(10000000, 99999999).ToString(),
                    Precio = item.Precio,
                    Stock = 0,
                    EsPerecedero = false
                };
                _controladoraProducto.AgregarProducto(producto);

                var prodCreado = _controladoraProducto.ObtenerProductos()
                    .OrderByDescending(p => p.Id).FirstOrDefault(p => p.Nombre == item.NombreProducto);
                if (prodCreado == null) throw new Exception("Error al crear el producto.");

                _gestor.AsignarProductoAProveedor(item.ProveedorId, prodCreado.Id, prodCreado.Nombre);

                var compra = new Compra
                {
                    ProveedorId = item.ProveedorId,
                    FormaPago = "Transferencia",
                    NumeroFactura = numeroFactura,
                    FechaCompra = DateTime.Now
                };
                compra.Detalles.Add(new DetalleCompra
                {
                    ProductoId = prodCreado.Id,
                    ProductoNombre = prodCreado.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = cantidad * precioUnitario
                });
                _controladoraCompra.RealizarCompra(compra);

                item.Estado = "Aprobado";
                ctx.SaveChanges();

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Aprobó Catálogo", $"Producto '{item.NombreProducto}' — Proveedor: {item.Proveedor?.RazonSocial}", ip);

                TempData["Success"] = $"Producto '{item.NombreProducto}' aprobado y compra registrada correctamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "proveedores";
            return RedirectToAction("Index", "Admin");
        }

        [AdminOnlyFilter]
        [HttpPost]
        public IActionResult Rechazar(int id, string __panel)
        {
            try
            {
                _controladora.CambiarEstado(id, "Rechazado");

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Rechazó Catálogo", $"Oferta ID: {id}", ip);

                TempData["Success"] = "Oferta rechazada.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "proveedores";
            return RedirectToAction("Index", "Admin");
        }

        [AdminOnlyFilter]
        [HttpGet]
        public JsonResult ObtenerPendientes()
        {
            var pendientes = _controladora.ObtenerPendientes();
            return Json(new
            {
                cantidad = pendientes.Count,
                items = pendientes.Select(i => new {
                    i.Id,
                    i.NombreProducto,
                    i.Precio,
                    i.Descripcion,
                    proveedorNombre = i.Proveedor?.RazonSocial ?? "—",
                    fecha = i.FechaOferta.ToString("dd/MM/yyyy")
                })
            });
        }
    }
}