using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraFacturacion
    {
        private static ControladoraFacturacion _instancia;
        public static ControladoraFacturacion ObtenerInstancia()
        {
            if (_instancia == null) _instancia = new ControladoraFacturacion();
            return _instancia;
        }

        public void GuardarFactura(FacturaAfip factura)
        {
            using var ctx = new Contexto();
            ctx.FacturasAfip.Add(factura);
            ctx.SaveChanges();
        }

        public FacturaAfip ObtenerPorVenta(int ventaId)
        {
            using var ctx = new Contexto();
            return ctx.FacturasAfip
                .Include(f => f.Venta)
                .FirstOrDefault(f => f.VentaId == ventaId);
        }

        public List<FacturaAfip> ObtenerTodas()
        {
            using var ctx = new Contexto();
            return ctx.FacturasAfip
                .Include(f => f.Venta)
                    .ThenInclude(v => v.Cliente)
                .OrderByDescending(f => f.FechaEmision)
                .AsNoTracking()
                .ToList();
        }

        public bool TieneFactura(int ventaId)
        {
            using var ctx = new Contexto();
            return ctx.FacturasAfip.Any(f => f.VentaId == ventaId);
        }
    }
}