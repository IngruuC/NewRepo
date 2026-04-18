using ENTIDADES;
using Microsoft.EntityFrameworkCore;
using MODELO;

namespace CONTROLADORA
{
    public class ControladoraSuscripcion
    {
        // Igual que las demás: instancia nueva con contexto por operación
        public ControladoraSuscripcion() { }

        // ════════════════════════════════════════════════════
        // PLANES
        // ════════════════════════════════════════════════════

        public List<PlanSuscripcion> ObtenerPlanes()
        {
            using var ctx = new Contexto();
            return ctx.PlanesSuscripcion
                .Where(p => p.Activo)
                .OrderBy(p => p.DuracionMeses)
                .ToList();
        }

        // ════════════════════════════════════════════════════
        // SUSCRIPCIONES
        // ════════════════════════════════════════════════════

        public SuscripcionCliente ObtenerSuscripcionActiva(int clienteId)
        {
            using var ctx = new Contexto();
            return ctx.SuscripcionesCliente
                .Include(s => s.Plan)
                .Where(s => s.ClienteId == clienteId
                         && s.Estado == "Activa"
                         && s.FechaVencimiento > DateTime.Now)
                .OrderByDescending(s => s.FechaVencimiento)
                .FirstOrDefault();
        }

        public List<SuscripcionCliente> ObtenerTodasSuscripciones()
        {
            using var ctx = new Contexto();
            return ctx.SuscripcionesCliente
                .Include(s => s.Cliente)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.FechaCreacion)
                .ToList();
        }

        public (bool ok, string mensaje) ActivarManual(int clienteId, int planId)
        {
            using var ctx = new Contexto();

            var plan = ctx.PlanesSuscripcion.Find(planId);
            if (plan == null) return (false, "Plan no encontrado.");

            // Cancelar suscripción activa anterior si existe
            var anterior = ctx.SuscripcionesCliente
                .FirstOrDefault(s => s.ClienteId == clienteId
                                  && s.Estado == "Activa"
                                  && s.FechaVencimiento > DateTime.Now);
            if (anterior != null)
            {
                anterior.Estado = "Cancelada";
                ctx.SaveChanges();
            }

            ctx.SuscripcionesCliente.Add(new SuscripcionCliente
            {
                ClienteId = clienteId,
                PlanId = planId,
                FechaInicio = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddMonths(plan.DuracionMeses),
                Estado = "Activa",
                Origen = "Admin",
                FechaCreacion = DateTime.Now
            });
            ctx.SaveChanges();
            return (true, $"Suscripción '{plan.Nombre}' activada manualmente.");
        }

        public (bool ok, string mensaje) DesactivarManual(int clienteId)
        {
            using var ctx = new Contexto();

            var suscripcion = ctx.SuscripcionesCliente
                .FirstOrDefault(s => s.ClienteId == clienteId
                                  && s.Estado == "Activa"
                                  && s.FechaVencimiento > DateTime.Now);

            if (suscripcion == null) return (false, "El cliente no tiene suscripción activa.");

            suscripcion.Estado = "Cancelada";
            ctx.SaveChanges();
            return (true, "Suscripción cancelada correctamente.");
        }

        public void ActualizarVencimientos()
        {
            using var ctx = new Contexto();
            var vencidas = ctx.SuscripcionesCliente
                .Where(s => s.Estado == "Activa" && s.FechaVencimiento <= DateTime.Now)
                .ToList();

            foreach (var s in vencidas)
                s.Estado = "Vencida";

            if (vencidas.Any())
                ctx.SaveChanges();
        }

        // ════════════════════════════════════════════════════
        // PAGOS
        // ════════════════════════════════════════════════════

        public (bool ok, string mensaje) SolicitarSuscripcion(int clienteId, int planId)
        {
            using var ctx = new Contexto();

            var plan = ctx.PlanesSuscripcion.Find(planId);
            if (plan == null) return (false, "Plan no encontrado.");

            var pendiente = ctx.PagosSuscripcion
                .Any(p => p.ClienteId == clienteId && p.Estado == "Pendiente");
            if (pendiente) return (false, "Ya tenés una solicitud pendiente de aprobación.");

            ctx.PagosSuscripcion.Add(new PagoSuscripcion
            {
                ClienteId = clienteId,
                PlanId = planId,
                Monto = plan.Precio,
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente"
            });
            ctx.SaveChanges();
            return (true, "Solicitud enviada. En breve el administrador aprobará tu pago.");
        }

        public (bool ok, string mensaje) AprobarPago(int pagoId)
        {
            using var ctx = new Contexto();

            var pago = ctx.PagosSuscripcion
                .Include(p => p.Plan)
                .FirstOrDefault(p => p.Id == pagoId);

            if (pago == null) return (false, "Pago no encontrado.");
            if (pago.Estado != "Pendiente") return (false, "El pago ya fue procesado.");

            pago.Estado = "Aprobado";
            pago.FechaAprobacion = DateTime.Now;

            // Cancelar suscripción activa anterior
            var anterior = ctx.SuscripcionesCliente
                .FirstOrDefault(s => s.ClienteId == pago.ClienteId
                                  && s.Estado == "Activa"
                                  && s.FechaVencimiento > DateTime.Now);
            if (anterior != null)
                anterior.Estado = "Cancelada";

            ctx.SuscripcionesCliente.Add(new SuscripcionCliente
            {
                ClienteId = pago.ClienteId,
                PlanId = pago.PlanId,
                FechaInicio = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddMonths(pago.Plan.DuracionMeses),
                Estado = "Activa",
                Origen = "Pago",
                FechaCreacion = DateTime.Now
            });

            ctx.SaveChanges();
            return (true, $"Pago aprobado. Suscripción '{pago.Plan.Nombre}' activada.");
        }

        public (bool ok, string mensaje) RechazarPago(int pagoId, string observacion)
        {
            using var ctx = new Contexto();

            var pago = ctx.PagosSuscripcion.Find(pagoId);
            if (pago == null) return (false, "Pago no encontrado.");
            if (pago.Estado != "Pendiente") return (false, "El pago ya fue procesado.");

            pago.Estado = "Rechazado";
            pago.FechaAprobacion = DateTime.Now;
            pago.Observacion = observacion;

            ctx.SaveChanges();
            return (true, "Pago rechazado.");
        }

        public List<PagoSuscripcion> ObtenerTodosPagos()
        {
            using var ctx = new Contexto();
            return ctx.PagosSuscripcion
                .Include(p => p.Cliente)
                .Include(p => p.Plan)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToList();
        }

        public int ContarPagosPendientes()
        {
            using var ctx = new Contexto();
            return ctx.PagosSuscripcion.Count(p => p.Estado == "Pendiente");
        }

        public List<PagoSuscripcion> ObtenerPagosCliente(int clienteId)
        {
            using var ctx = new Contexto();
            return ctx.PagosSuscripcion
                .Include(p => p.Plan)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToList();
        }
    }
}