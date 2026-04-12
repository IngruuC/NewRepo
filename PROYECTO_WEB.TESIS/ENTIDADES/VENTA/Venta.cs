using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{


    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("ClienteId")] // Especificar el nombre exacto de la columna
        public int ClienteId { get; set; }

        [Required]
        [Column("Fecha")] // Cambiar a "Fecha" en lugar de "FechaVenta"
        public DateTime FechaVenta { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Total { get; set; }

        [Required]
        public string FormaPago { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }
        public virtual ICollection<DetalleVenta> Detalles { get; set; }

        public Venta()
        {
            Detalles = new List<DetalleVenta>();
        }
    }
}