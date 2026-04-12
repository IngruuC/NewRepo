using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{

    [Table("DetallesVenta")]
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; } //

        [Required]
        public int VentaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Subtotal { get; set; }

        [Required]
        public string ProductoNombre { get; set; }

        public virtual Venta Venta { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
    }
}
