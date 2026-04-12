using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class ClienteController : Controller
    {
        private readonly ControladoraCliente _controladora;
        private readonly ControladoraUsuario _controladoraUsuario;

        public ClienteController()
        {
            _controladora = ControladoraCliente.ObtenerInstancia();
            _controladoraUsuario = ControladoraUsuario.ObtenerInstancia();
        }

        [HttpPost]
        public IActionResult Crear(Cliente cliente, string __panel)
        {
            try
            {
                _controladora.AgregarCliente(cliente);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Nuevo Cliente",
                    $"{cliente.Nombre} {cliente.Apellido} — Doc: {cliente.Documento}", ip);

                TempData["Success"] = "Cliente creado exitosamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "clientes";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Editar(Cliente cliente, string __panel)
        {
            try
            {
                _controladora.ModificarCliente(cliente);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Modificó Cliente",
                    $"{cliente.Nombre} {cliente.Apellido} — ID: {cliente.Id}", ip);

                TempData["Success"] = "Cliente actualizado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "clientes";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult Eliminar(int id, string __panel)
        {
            try
            {
                _controladora.EliminarCliente(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Eliminó Cliente",
                    $"Cliente ID: {id}", ip);

                TempData["Success"] = "Cliente eliminado.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "clientes";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult AsignarUsuarioDefault(int id, string __panel)
        {
            try
            {
                _controladoraUsuario.AsignarUsuarioDefaultACliente(id);

                var usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Desconocido";
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Asignó Usuario Default",
                    $"Cliente ID: {id}", ip);

                TempData["Success"] = "Usuario asignado correctamente.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            TempData["ActivePanel"] = __panel ?? "clientes";
            return RedirectToAction("Index", "Admin");
        }
    }
}