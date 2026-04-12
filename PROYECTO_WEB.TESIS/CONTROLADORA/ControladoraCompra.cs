using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraCompra
    {
        private static ControladoraCompra instancia;
        private Contexto contexto;

        private ControladoraCompra()
        {
            contexto = new Contexto();
        }

        public static ControladoraCompra ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraCompra();
            return instancia;
        }

        public void RealizarCompra(Compra compra)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var proveedor = contexto.Proveedores.Find(compra.ProveedorId);
                if (proveedor == null)
                    throw new Exception("Proveedor no encontrado.");

                var nuevaCompra = new Compra
                {
                    ProveedorId = compra.ProveedorId,
                    FechaCompra = DateTime.Now,
                    FormaPago = compra.FormaPago,
                    NumeroFactura = compra.NumeroFactura
                };

                decimal total = 0;
                foreach (var detalle in compra.Detalles)
                {
                    var producto = contexto.Productos.Find(detalle.ProductoId);
                    if (producto == null)
                        throw new Exception($"Producto no encontrado: {detalle.ProductoId}");

                    var nuevoDetalle = new DetalleCompra
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        ProductoNombre = producto.Nombre,
                        Subtotal = detalle.PrecioUnitario * detalle.Cantidad
                    };

                    total += nuevoDetalle.Subtotal;
                    nuevaCompra.Detalles.Add(nuevoDetalle);

                    producto.Stock += detalle.Cantidad;
                    contexto.Entry(producto).State = EntityState.Modified;
                }

                nuevaCompra.Total = total;
                contexto.Compras.Add(nuevaCompra);
                contexto.SaveChanges();
                transaction.Commit();

                compra.Id = nuevaCompra.Id;
                compra.Total = nuevaCompra.Total;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al realizar la compra: {ex.Message}");
            }
        }

        public List<Compra> ObtenerCompras()
        {
            return contexto.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToList();
        }

        public Compra ObtenerCompraPorId(int id)
        {
            return contexto.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefault(c => c.Id == id);
        }

        public List<Compra> ObtenerComprasPorFecha(DateTime inicio, DateTime fin)
        {
            return contexto.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .Where(c => c.FechaCompra >= inicio && c.FechaCompra <= fin)
                .OrderBy(c => c.FechaCompra)
                .ToList();
        }

        public void EliminarCompra(int id)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var compra = contexto.Compras
                    .Include(c => c.Detalles)
                    .FirstOrDefault(c => c.Id == id);

                if (compra == null)
                    throw new Exception("Compra no encontrada.");

                foreach (var detalle in compra.Detalles)
                {
                    var producto = contexto.Productos.Find(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad;
                        if (producto.Stock < 0)
                            throw new Exception($"No se puede eliminar: {producto.Nombre} quedaría con stock negativo.");
                    }
                }

                contexto.Compras.Remove(compra);
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