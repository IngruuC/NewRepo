namespace ENTIDADES
{
    public class Promocion
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string TipoDescuento { get; set; } // "Porcentaje" o "Monto"
        public decimal ValorDescuento { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; } = true;
        public string Descripcion { get; set; }

        // Navegación
        public Producto Producto { get; set; }

        // Método helper para calcular precio final
        public decimal AplicarDescuento(decimal precioOriginal)
        {
            if (TipoDescuento == "Porcentaje")
                return precioOriginal - (precioOriginal * ValorDescuento / 100);
            else
                return precioOriginal - ValorDescuento;
        }

        // Verifica si la promo está vigente
        public bool EstaVigente()
        {
            return Activa && DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;
        }
    }
}