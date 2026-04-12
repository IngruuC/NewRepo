using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ENTIDADES
{
    [Table("Compra")]
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProveedorId { get; set; }

        [Required]
        public DateTime FechaCompra { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Total { get; set; }

        [Required]
        public string FormaPago { get; set; }

        [Required]
        public string NumeroFactura { get; set; }

        [ForeignKey("ProveedorId")]
        public virtual Proveedor Proveedor { get; set; }
        public virtual ICollection<DetalleCompra> Detalles { get; set; }

        public Compra()
        {
            Detalles = new List<DetalleCompra>();
        }
    }
}