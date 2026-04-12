// Controllers/AuditoriaController.cs
using CONTROLADORA;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class AuditoriaController : Controller
    {
        private readonly ControladoraAuditoria _controladora;

        public AuditoriaController()
        {
            _controladora = new ControladoraAuditoria();
        }

        // Devuelve JSON para filtrar inline sin redirigir
        [HttpGet]
        public JsonResult Filtrar(int? usuarioId, DateTime? desde, DateTime? hasta)
        {
            var sesiones = _controladora.ObtenerHistorialSesiones(usuarioId, desde, hasta);
            var data = sesiones.Select(s => new {
                nombreUsuario = s.NombreUsuario,
                fechaIngreso = s.FechaIngreso.ToString("dd/MM/yyyy HH:mm"),
                fechaSalida = s.FechaSalida?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                // Fix IP: mapear ::1 a 127.0.0.1 (localhost IPv6)
                direccionIP = NormalizarIP(s.DireccionIP),
                dispositivo = s.Dispositivo,
                tipoSesion = s.TipoSesion,
                sesionActiva = s.SesionActiva
            }).ToList();

            return Json(data);
        }

        private static string NormalizarIP(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return "—";
            // ::1 es localhost IPv6 → mostrar como 127.0.0.1
            if (ip == "::1") return "127.0.0.1";
            // ::ffff:127.0.0.1 también es localhost
            if (ip.StartsWith("::ffff:")) return ip.Substring(7);
            return ip;
        }
    }
}