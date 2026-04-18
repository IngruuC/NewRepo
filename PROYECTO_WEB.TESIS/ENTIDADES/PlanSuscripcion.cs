using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class PlanSuscripcion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }           // "Mensual", "Trimestral", etc.
        public int DuracionMeses { get; set; }       // 1, 3, 6, 12
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Navegación
        public List<SuscripcionCliente> Suscripciones { get; set; } = new();
        public List<PagoSuscripcion> Pagos { get; set; } = new();
    }
}
