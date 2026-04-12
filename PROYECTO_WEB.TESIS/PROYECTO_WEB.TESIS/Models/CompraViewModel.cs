namespace PROYECTO_WEB.TESIS.Models
{
    public class CompraViewModel
    {
        public int ProveedorId { get; set; }
        public string FormaPago { get; set; }
        public string NumeroFactura { get; set; }
        public List<DetalleCompraViewModel> Detalles { get; set; } = new();
    }

    public class DetalleCompraViewModel
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}