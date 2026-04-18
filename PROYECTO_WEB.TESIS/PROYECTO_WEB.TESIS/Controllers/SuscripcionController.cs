using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Helpers;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class SuscripcionController : Controller
    {
        private readonly ControladoraSuscripcion _ctrl;
        private readonly ControladoraCliente _ctrlCliente;

        public SuscripcionController()
        {
            _ctrl = new ControladoraSuscripcion();
            _ctrlCliente = ControladoraCliente.ObtenerInstancia();
        }

        // ════════════════════════════════════════════════════
        // ADMIN — obtener todos los datos para el panel
        // GET /Suscripcion/ObtenerDatosAdmin
        // ════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult ObtenerDatosAdmin()
        {
            if (!SessionHelper.EsAdministrador(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            _ctrl.ActualizarVencimientos();

            var suscripciones = _ctrl.ObtenerTodasSuscripciones().Select(s => new {
                s.Id,
                s.ClienteId,
                clienteNombre = s.Cliente?.Nombre + " " + s.Cliente?.Apellido,
                planNombre = s.Plan?.Nombre,
                fechaInicio = s.FechaInicio.ToString("dd/MM/yyyy"),
                fechaVencimiento = s.FechaVencimiento.ToString("dd/MM/yyyy"),
                s.Estado,
                s.Origen,
                diasRestantes = s.DiasRestantes
            });

            var pagos = _ctrl.ObtenerTodosPagos().Select(p => new {
                p.Id,
                p.ClienteId,
                clienteNombre = p.Cliente?.Nombre + " " + p.Cliente?.Apellido,
                planNombre = p.Plan?.Nombre,
                p.Monto,
                fechaSolicitud = p.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"),
                fechaAprobacion = p.FechaAprobacion?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                p.Estado,
                p.Observacion
            });

            var planes = _ctrl.ObtenerPlanes().Select(p => new {
                p.Id,
                p.Nombre,
                p.DuracionMeses,
                p.Precio,
                p.Descripcion
            });

            var clientes = _ctrlCliente.ObtenerClientes().Select(c => new {
                c.Id,
                nombre = c.Nombre + " " + c.Apellido
            });

            return Json(new
            {
                ok = true,
                suscripciones,
                pagos,
                planes,
                clientes,
                pendientes = _ctrl.ContarPagosPendientes()
            });
        }

        // ════════════════════════════════════════════════════
        // ADMIN — activar manualmente
        // POST /Suscripcion/ActivarManual
        // ════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult ActivarManual(int clienteId, int planId)
        {
            if (!SessionHelper.EsAdministrador(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            var (ok, mensaje) = _ctrl.ActivarManual(clienteId, planId);

            if (ok)
            {
                var usuario = SessionHelper.GetUsuarioNombre(HttpContext.Session) ?? "Admin";
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Activó Suscripción",
                    $"Cliente ID: {clienteId} — Plan ID: {planId} (Manual)",
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return Json(new { ok, mensaje });
        }

        // ════════════════════════════════════════════════════
        // ADMIN — desactivar manualmente
        // POST /Suscripcion/DesactivarManual
        // ════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult DesactivarManual(int clienteId)
        {
            if (!SessionHelper.EsAdministrador(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            var (ok, mensaje) = _ctrl.DesactivarManual(clienteId);

            if (ok)
            {
                var usuario = SessionHelper.GetUsuarioNombre(HttpContext.Session) ?? "Admin";
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Desactivó Suscripción",
                    $"Cliente ID: {clienteId}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return Json(new { ok, mensaje });
        }

        // ════════════════════════════════════════════════════
        // ADMIN — aprobar pago
        // POST /Suscripcion/AprobarPago
        // ════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult AprobarPago(int pagoId)
        {
            if (!SessionHelper.EsAdministrador(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            var (ok, mensaje) = _ctrl.AprobarPago(pagoId);

            if (ok)
            {
                var usuario = SessionHelper.GetUsuarioNombre(HttpContext.Session) ?? "Admin";
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Aprobó Pago Suscripción",
                    $"Pago ID: {pagoId}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return Json(new { ok, mensaje });
        }

        // ════════════════════════════════════════════════════
        // ADMIN — rechazar pago
        // POST /Suscripcion/RechazarPago
        // ════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult RechazarPago(int pagoId, string observacion = "")
        {
            if (!SessionHelper.EsAdministrador(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            var (ok, mensaje) = _ctrl.RechazarPago(pagoId, observacion);

            if (ok)
            {
                var usuario = SessionHelper.GetUsuarioNombre(HttpContext.Session) ?? "Admin";
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Rechazó Pago Suscripción",
                    $"Pago ID: {pagoId} — Obs: {observacion}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return Json(new { ok, mensaje });
        }

        // ════════════════════════════════════════════════════
        // CLIENTE — obtener datos de su suscripción
        // GET /Suscripcion/MiSuscripcion
        // ════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult MiSuscripcion()
        {
            if (!SessionHelper.EsCliente(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            var cliente = _ctrlCliente.ObtenerClientes()
                .FirstOrDefault(c => c.UsuarioId == usuarioId);

            if (cliente == null)
                return Json(new { ok = false, error = "Cliente no encontrado" });

            _ctrl.ActualizarVencimientos();

            var suscripcion = _ctrl.ObtenerSuscripcionActiva(cliente.Id);
            var planes = _ctrl.ObtenerPlanes();
            var historial = _ctrl.ObtenerPagosCliente(cliente.Id);

            return Json(new
            {
                ok = true,
                tieneSuscripcion = suscripcion != null,
                suscripcion = suscripcion == null ? null : new
                {
                    planNombre = suscripcion.Plan?.Nombre,
                    fechaInicio = suscripcion.FechaInicio.ToString("dd/MM/yyyy"),
                    fechaVencimiento = suscripcion.FechaVencimiento.ToString("dd/MM/yyyy"),
                    suscripcion.Origen,
                    suscripcion.DiasRestantes
                },
                planes = planes.Select(p => new {
                    p.Id,
                    p.Nombre,
                    p.DuracionMeses,
                    p.Precio,
                    p.Descripcion
                }),
                historial = historial.Select(p => new {
                    planNombre = p.Plan?.Nombre,
                    p.Monto,
                    fechaSolicitud = p.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"),
                    fechaAprobacion = p.FechaAprobacion?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                    p.Estado,
                    p.Observacion
                })
            });
        }

        // ════════════════════════════════════════════════════
        // CLIENTE — solicitar suscripción
        // POST /Suscripcion/Solicitar
        // ════════════════════════════════════════════════════
        [HttpPost]
        public IActionResult Solicitar(int planId)
        {
            if (!SessionHelper.EsCliente(HttpContext.Session))
                return Json(new { ok = false, error = "No autorizado" });

            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            var cliente = _ctrlCliente.ObtenerClientes()
                .FirstOrDefault(c => c.UsuarioId == usuarioId);

            if (cliente == null)
                return Json(new { ok = false, error = "Cliente no encontrado" });

            var (ok, mensaje) = _ctrl.SolicitarSuscripcion(cliente.Id, planId);
            return Json(new { ok, mensaje });
        }
    }
}