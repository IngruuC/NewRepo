using ENTIDADES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VISTA
{
    public class ItemCarritoTemp
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal => Producto.Precio * Cantidad;
    }

    public static class CarritoTemporal
    {
        private static List<ItemCarritoTemp> items = new List<ItemCarritoTemp>();

        public static void AgregarProducto(Producto producto, int cantidad = 1)
        {
            // Verificar stock antes de agregar
            if (producto.Stock < cantidad)
                throw new Exception($"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles.");

            var itemExistente = items.FirstOrDefault(i => i.Producto.Id == producto.Id);

            if (itemExistente != null)
            {
                // Verificar que no exceda el stock disponible
                if (itemExistente.Cantidad + cantidad > producto.Stock)
                    throw new Exception($"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles.");

                itemExistente.Cantidad += cantidad;
            }
            else
            {
                items.Add(new ItemCarritoTemp { Producto = producto, Cantidad = cantidad });
            }
        }

        public static void EliminarProducto(int productoId)
        {
            var item = items.FirstOrDefault(i => i.Producto.Id == productoId);
            if (item != null)
                items.Remove(item);
        }

        public static void ModificarCantidad(int productoId, int nuevaCantidad)
        {
            var item = items.FirstOrDefault(i => i.Producto.Id == productoId);
            if (item != null)
            {
                // Verificar que no exceda el stock disponible
                if (nuevaCantidad > item.Producto.Stock)
                    throw new Exception($"Stock insuficiente. Solo hay {item.Producto.Stock} unidades disponibles.");

                if (nuevaCantidad <= 0)
                    items.Remove(item);
                else
                    item.Cantidad = nuevaCantidad;
            }
        }

        public static List<ItemCarritoTemp> ObtenerItems()
        {
            return items.ToList();
        }

        public static decimal ObtenerTotal()
        {
            return items.Sum(i => i.Subtotal);
        }

        public static int ObtenerCantidadTotal()
        {
            return items.Sum(i => i.Cantidad);
        }

        public static void VaciarCarrito()
        {
            items.Clear();
        }
    }
}