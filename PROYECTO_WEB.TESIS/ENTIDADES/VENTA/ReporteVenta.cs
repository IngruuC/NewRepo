using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class ReporteVenta           //REVISAR
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalTransacciones { get; set; }
        public List<Venta> Ventas { get; set; }
        public Dictionary<string, decimal> VentasPorFormaPago { get; set; }
        public Dictionary<string, int> ProductosMasVendidos { get; set; }
    }
}