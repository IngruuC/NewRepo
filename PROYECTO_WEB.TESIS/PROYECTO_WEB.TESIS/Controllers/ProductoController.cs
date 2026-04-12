using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class ProductoController : Controller
    {
        private readonly ControladoraProducto _controladora;
        public ProductoController() { _controladora = ControladoraProducto.ObtenerInstancia(); }

        [HttpPost]
        public IActionResult Crear(Producto producto, string __panel)
        {
            try
            {
                _controladora.AgregarProducto(producto);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Nuevo Producto", $"{producto.Nombre} — Precio: ${producto.Precio:N2}", ip);

                TempData["Success"] = "Producto creado exitosamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "productos";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Editar(Producto producto, string __panel)
        {
            try
            {
                _controladora.ModificarProducto(producto);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Modificó Producto", $"{producto.Nombre} — ID: {producto.Id}", ip);

                TempData["Success"] = "Producto actualizado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "productos";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id, string __panel)
        {
            try
            {
                _controladora.EliminarProducto(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Eliminó Producto", $"Producto ID: {id}", ip);

                TempData["Success"] = "Producto eliminado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "productos";
            return RedirectToAction("Index", "Admin");
        }
    }
}