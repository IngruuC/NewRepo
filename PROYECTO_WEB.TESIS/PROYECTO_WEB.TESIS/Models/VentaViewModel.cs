namespace PROYECTO_WEB.TESIS.Models
{
    public class VentaViewModel
    {
        public int ClienteId { get; set; }
        public string FormaPago { get; set; }
        public List<DetalleVentaViewModel> Detalles { get; set; } = new();
    }

    public class DetalleVentaViewModel
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}