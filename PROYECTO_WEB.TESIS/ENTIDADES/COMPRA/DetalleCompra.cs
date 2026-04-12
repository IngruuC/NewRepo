using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENTIDADES
{
    [Table("DetallesCompra")]
    public class DetalleCompra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("CompraId")]
        public int CompraId { get; set; }

        [Required]
        [Column("ProductoId")]
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


        [ForeignKey("CompraId")]
        public virtual Compra Compra { get; set; }
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
    }
}