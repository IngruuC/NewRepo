using CONTROLADORA;
using ENTIDADES;
using ENTIDADES.SEGURIDAD;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class UsuarioController : Controller
    {
        private readonly ControladoraSeguridad _controladoraSeguridad;

        public UsuarioController()
        {
            _controladoraSeguridad = ControladoraSeguridad.Instancia;
        }

        [HttpPost]
        public IActionResult ResetearClave(int id)
        {
            var usuario = _controladoraSeguridad.RecuperarUsuarios().FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                var msg = _controladoraSeguridad.ResetearClave(usuario);
                TempData["Success"] = msg;
            }
            TempData["ActivePanel"] = "seguridad";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var usuario = _controladoraSeguridad.RecuperarUsuarios().FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                var msg = _controladoraSeguridad.EliminarUsuario(usuario);
                TempData[msg.Contains("Error") || msg.Contains("No se puede") ? "Error" : "Success"] = msg;
            }
            TempData["ActivePanel"] = "seguridad";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Modificar(int id, string email, string estado, string __panel)
        {
            try
            {
                var usuario = _controladoraSeguridad.RecuperarUsuarios().FirstOrDefault(u => u.Id == id);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                }
                else
                {
                    usuario.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
                    usuario.Estado = estado == "true";
                    var msg = _controladoraSeguridad.ModificarUsuario(usuario);
                    TempData[msg.Contains("exitosamente") ? "Success" : "Error"] = msg;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al modificar: " + ex.Message;
            }
            TempData["ActivePanel"] = __panel ?? "seguridad";
            return RedirectToAction("Index", "Admin");
        }
    }
}