using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class PromocionController : Controller
    {
        private readonly ControladoraPromocion _controladora;
        private readonly ControladoraProducto _controladoraProducto;

        public PromocionController()
        {
            _controladora = ControladoraPromocion.ObtenerInstancia();
            _controladoraProducto = ControladoraProducto.ObtenerInstancia();
        }

        [HttpGet]
        public JsonResult ObtenerVigentes()
        {
            var promos = _controladora.ObtenerVigentes();
            return Json(promos.Select(p => new {
                p.Id,
                p.ProductoId,
                productoNombre = p.Producto?.Nombre ?? "—",
                p.TipoDescuento,
                p.ValorDescuento,
                fechaFin = p.FechaFin.ToString("dd/MM/yyyy HH:mm"),
                p.Descripcion
            }));
        }

        [HttpGet]
        public JsonResult ObtenerTodas()
        {
            var promos = _controladora.ObtenerTodas();
            var ahora = DateTime.Now;
            return Json(promos.Select(p => new {
                p.Id,
                p.ProductoId,
                productoNombre = p.Producto?.Nombre ?? "—",
                p.TipoDescuento,
                p.ValorDescuento,
                fechaInicio = p.FechaInicio.ToString("dd/MM/yyyy"),
                fechaFin = p.FechaFin.ToString("dd/MM/yyyy"),
                p.Activa,
                vigente = p.Activa && p.FechaInicio <= ahora && p.FechaFin >= ahora,
                p.Descripcion
            }));
        }

        [HttpPost]
        public IActionResult Crear(Promocion promocion, string __panel)
        {
            try
            {
                if (promocion.ValorDescuento <= 0) throw new Exception("El valor del descuento debe ser mayor a 0.");
                if (promocion.TipoDescuento == "Porcentaje" && promocion.ValorDescuento > 100) throw new Exception("El porcentaje no puede ser mayor a 100.");
                if (promocion.FechaFin <= DateTime.Now) throw new Exception("La fecha de fin debe ser futura.");

                var promoActual = _controladora.ObtenerVigentePorProducto(promocion.ProductoId);
                if (promoActual != null) _controladora.Desactivar(promoActual.Id);
                _controladora.Agregar(promocion);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var descuento = promocion.TipoDescuento == "Porcentaje" ? $"{promocion.ValorDescuento}%" : $"${promocion.ValorDescuento:N2}";
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Nueva Promoción", $"Producto ID: {promocion.ProductoId} — Descuento: {descuento}", ip);

                TempData["Success"] = "Promoción creada correctamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "promociones";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Desactivar(int id, string __panel)
        {
            try
            {
                _controladora.Desactivar(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Desactivó Promoción", $"Promoción ID: {id}", ip);

                TempData["Success"] = "Promoción desactivada.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "promociones";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id, string __panel)
        {
            try
            {
                _controladora.Eliminar(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Eliminó Promoción", $"Promoción ID: {id}", ip);

                TempData["Success"] = "Promoción eliminada.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "promociones";
            return RedirectToAction("Index", "Admin");
        }
    }
}