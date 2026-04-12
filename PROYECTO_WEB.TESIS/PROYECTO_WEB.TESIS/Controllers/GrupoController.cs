// Controllers/GrupoController.cs
using CONTROLADORA;
using ENTIDADES;
using ENTIDADES.SEGURIDAD;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class GrupoController : Controller
    {
        private readonly ControladoraSeguridad _controladora;

        public GrupoController()
        {
            _controladora = ControladoraSeguridad.Instancia;
        }

        [HttpPost]
        public IActionResult Crear(Grupo grupo)
        {
            try
            {
                var msg = _controladora.AgregarGrupo(grupo);
                TempData[msg.Contains("exitosamente") ? "Success" : "Error"] = msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Editar(Grupo grupo)
        {
            try
            {
                var msg = _controladora.ModificarGrupo(grupo);
                TempData[msg.Contains("exitosamente") ? "Success" : "Error"] = msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var grupo = _controladora.RecuperarGrupos().FirstOrDefault(g => g.Id == id);
                if (grupo != null)
                {
                    var msg = _controladora.EliminarGrupo(grupo);
                    TempData[msg.Contains("exitosamente") ? "Success" : "Error"] = msg;
                }
                else
                {
                    TempData["Error"] = "Grupo no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}