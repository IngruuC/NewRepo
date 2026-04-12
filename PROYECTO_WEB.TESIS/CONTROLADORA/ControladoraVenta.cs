using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraVenta
    {
        private static ControladoraVenta instancia;
        private Contexto contexto;

        private ControladoraVenta()
        {
            contexto = new Contexto();
        }

        public static ControladoraVenta ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraVenta();
            return instancia;
        }

        public void RealizarVenta(Venta venta)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var cliente = contexto.Clientes.Find(venta.ClienteId);
                if (cliente == null)
                    throw new Exception("Cliente no encontrado.");

                var nuevaVenta = new Venta
                {
                    ClienteId = venta.ClienteId,
                    FechaVenta = DateTime.Now,
                    FormaPago = venta.FormaPago
                };

                decimal totalOriginal = 0;
                foreach (var detalle in venta.Detalles)
                {
                    var producto = contexto.Productos.Find(detalle.ProductoId);
                    if (producto == null)
                        throw new Exception($"Producto no encontrado: {detalle.ProductoId}");

                    if (producto.Stock < detalle.Cantidad)
                        throw new Exception($"Stock insuficiente para: {producto.Nombre}");

                    var nuevoDetalle = new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = producto.Precio,
                        ProductoNombre = producto.Nombre,
                        Subtotal = producto.Precio * detalle.Cantidad
                    };

                    totalOriginal += nuevoDetalle.Subtotal;
                    nuevaVenta.Detalles.Add(nuevoDetalle);

                    producto.Stock -= detalle.Cantidad;
                    contexto.Entry(producto).State = EntityState.Modified;
                }

                nuevaVenta.Total = nuevaVenta.FormaPago switch
                {
                    "Efectivo" => totalOriginal * 0.85m,
                    "Tarjeta de Crédito" => totalOriginal * 1.10m,
                    _ => totalOriginal
                };

                contexto.Ventas.Add(nuevaVenta);
                contexto.SaveChanges();
                transaction.Commit();

                venta.Id = nuevaVenta.Id;
                venta.Total = nuevaVenta.Total;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al realizar la venta: {ex.Message}");
            }
        }

        public List<Venta> ObtenerVentas()
        {
            return contexto.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToList();
        }

        public Venta ObtenerVentaPorId(int id)
        {
            return contexto.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.Id == id);
        }

        public List<Venta> ObtenerVentasPorFecha(DateTime inicio, DateTime fin)
        {
            return contexto.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= inicio && v.FechaVenta <= fin)
                .OrderBy(v => v.FechaVenta)
                .ToList();
        }

        public void EliminarVenta(int id)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var venta = contexto.Ventas
                    .Include(v => v.Detalles)
                    .FirstOrDefault(v => v.Id == id);

                if (venta == null)
                    throw new Exception("Venta no encontrada.");

                foreach (var detalle in venta.Detalles)
                {
                    var producto = contexto.Productos.Find(detalle.ProductoId);
                    if (producto != null)
                        producto.Stock += detalle.Cantidad;
                }

                contexto.Ventas.Remove(venta);
                contexto.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}