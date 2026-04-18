using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class PagoSuscripcion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [Required]
        public int PlanId { get; set; }

        [ForeignKey("PlanId")]
        public virtual PlanSuscripcion Plan { get; set; }

        [Required]
        [Column(TypeName = "numeric(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        // "Pendiente", "Aprobado", "Rechazado"
        [Required]
        public string Estado { get; set; } = "Pendiente";

        // Nullable — motivo de rechazo (opcional)
        public string? Observacion { get; set; }
    }
}
