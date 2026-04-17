using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ENTIDADES
{
    [Table("FacturasAfip")]
    public class FacturaAfip
    {
        [Key]
        public int Id { get; set; }

        // Relación con la venta
        [Required]
        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public virtual Venta Venta { get; set; }

        // Datos del comprobante
        [Required]
        public string TipoComprobante { get; set; } // "A" o "B"

        [Required]
        public int NroComprobante { get; set; }

        [Required]
        public string CAE { get; set; }

        [Required]
        public DateTime FechaVencimientoCAE { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "numeric(18,2)")]
        public decimal ImporteTotal { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal ImporteNeto { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal ImporteIVA { get; set; }

        // CUIT del emisor (el negocio)
        public string CuitEmisor { get; set; }

        // CUIT del receptor (cliente, puede ser null si es consumidor final)
        public string? CuitReceptor { get; set; }

        // Estado: "Emitida", "Error"
        public string Estado { get; set; } = "Emitida";

        // Mensaje de error si falló
        public string? MensajeError { get; set; }

        // Número de punto de venta
        public int PuntoVenta { get; set; } = 1;
    }
}