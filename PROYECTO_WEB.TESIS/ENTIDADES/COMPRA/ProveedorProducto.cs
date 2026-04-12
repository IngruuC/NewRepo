using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class ProveedorProducto
    {
        [Key]
        public int Id { get; set; }

        public int ProveedorId { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProveedorId")]
        public virtual Proveedor Proveedor { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }


        [Column(TypeName = "decimal")]
        public decimal PrecioCompra { get; set; } // Precio al que nos vende este proveedor
    }
}