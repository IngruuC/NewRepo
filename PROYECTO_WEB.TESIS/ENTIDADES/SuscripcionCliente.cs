using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class SuscripcionCliente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Activa"; // Activa, Vencida, Cancelada
        public string Origen { get; set; } = "Pago";   // "Pago" o "Admin"
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        public Cliente Cliente { get; set; }
        public PlanSuscripcion Plan { get; set; }

        // Helpers
        public bool EstaActiva => Estado == "Activa" && FechaVencimiento > DateTime.Now;
        public int DiasRestantes => EstaActiva ? (FechaVencimiento - DateTime.Now).Days : 0;
    }
}
