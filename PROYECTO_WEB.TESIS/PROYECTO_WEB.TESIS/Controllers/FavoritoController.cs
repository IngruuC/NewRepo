using CONTROLADORA;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class FavoritoController : Controller
    {
        private readonly ControladoraFavorito _controladora;
        private readonly ControladoraCliente _controladoraCliente;

        public FavoritoController()
        {
            _controladora = ControladoraFavorito.ObtenerInstancia();
            _controladoraCliente = ControladoraCliente.ObtenerInstancia();
        }

        private int? ObtenerClienteId()
        {
            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            if (usuarioId == null) return null;
            return _controladoraCliente.ObtenerClientes()
                .FirstOrDefault(c => c.UsuarioId == usuarioId)?.Id;
        }

        [HttpPost]
        public JsonResult Toggle([FromBody] int productoId)
        {
            try
            {
                var clienteId = ObtenerClienteId();
                if (clienteId == null) return Json(new { ok = false, error = "Cliente no encontrado." });

                var esFav = _controladora.EsFavorito(clienteId.Value, productoId);
                if (esFav)
                    _controladora.Eliminar(clienteId.Value, productoId);
                else
                    _controladora.Agregar(clienteId.Value, productoId);

                return Json(new { ok = true, esFavorito = !esFav });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerFavoritos()
        {
            try
            {
                var clienteId = ObtenerClienteId();
                if (clienteId == null) return Json(new { ok = false });

                var favs = _controladora.ObtenerPorCliente(clienteId.Value);
                return Json(new
                {
                    ok = true,
                    items = favs.Select(f => new
                    {
                        id = f.ProductoId,
                        nombre = f.Producto?.Nombre ?? "—",
                        precio = f.Producto?.Precio ?? 0,
                        stock = f.Producto?.Stock ?? 0
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }
    }
}