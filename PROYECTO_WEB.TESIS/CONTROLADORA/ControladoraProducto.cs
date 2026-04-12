using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraProducto
    {
        private static ControladoraProducto instancia;
        private Contexto contexto;

        private ControladoraProducto()
        {
            contexto = new Contexto();
        }

        public static ControladoraProducto ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraProducto();
            return instancia;
        }

        public void AgregarProducto(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.CodigoBarra) || producto.CodigoBarra.Length != 8)
                throw new Exception("El código de barras debe tener 8 dígitos.");

            if (contexto.Productos.Any(p => p.CodigoBarra == producto.CodigoBarra))
                throw new Exception("Ya existe un producto con ese código de barras.");

            if (producto.EsPerecedero && !producto.FechaVencimiento.HasValue)
                throw new Exception("Los productos perecederos deben tener fecha de vencimiento.");

            if (producto.Precio <= 0)
                throw new Exception("El precio debe ser mayor que cero.");

            if (producto.Stock < 0)
                throw new Exception("El stock no puede ser negativo.");

            contexto.Productos.Add(producto);
            contexto.SaveChanges();
        }

        public void ModificarProducto(Producto producto)
        {
            var productoExistente = contexto.Productos.Find(producto.Id);
            if (productoExistente == null)
                throw new Exception("Producto no encontrado.");

            if (contexto.Productos.Any(p => p.CodigoBarra == producto.CodigoBarra && p.Id != producto.Id))
                throw new Exception("Ya existe otro producto con ese código de barras.");

            productoExistente.Nombre = producto.Nombre;
            productoExistente.CodigoBarra = producto.CodigoBarra;
            productoExistente.EsPerecedero = producto.EsPerecedero;
            productoExistente.FechaVencimiento = producto.FechaVencimiento;
            productoExistente.Precio = producto.Precio;
            productoExistente.Stock = producto.Stock;

            contexto.SaveChanges();
        }

        public void EliminarProducto(int id)
        {
            var producto = contexto.Productos.Find(id);
            if (producto == null)
                throw new Exception("Producto no encontrado.");

            if (contexto.DetallesVenta.Any(d => d.ProductoId == id))
                throw new Exception("No se puede eliminar el producto porque tiene ventas asociadas.");

            contexto.Productos.Remove(producto);
            contexto.SaveChanges();
        }

        public List<Producto> ObtenerProductos()
        {
            return contexto.Productos.ToList();
        }

        public Producto ObtenerProductoPorId(int id)
        {
            return contexto.Productos.Find(id);
        }
    }
}