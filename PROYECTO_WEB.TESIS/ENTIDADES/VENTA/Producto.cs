using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{

    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string CodigoBarra { get; set; }

        public bool EsPerecedero { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; }

        public virtual ICollection<DetalleCompra> DetallesCompra { get; set; } //



        public Producto()
        {
            DetallesVenta = new List<DetalleVenta>();
            DetallesCompra = new List<DetalleCompra>();

        }
    }
}