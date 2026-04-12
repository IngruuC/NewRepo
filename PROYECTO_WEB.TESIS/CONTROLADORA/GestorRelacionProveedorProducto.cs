using ENTIDADES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CONTROLADORA
{
    public class GestorRelacionProveedorProducto
    {
        private static GestorRelacionProveedorProducto instancia;
        private Dictionary<int, List<RelacionProductoProveedor>> relaciones;
        private string archivoRelaciones = "relaciones_proveedor_producto.xml";

        public class RelacionProductoProveedor
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; }
        }

        [Serializable]
        public class DatosSerializables
        {
            public List<RegistroRelacion> Relaciones { get; set; }

            public DatosSerializables()
            {
                Relaciones = new List<RegistroRelacion>();
            }
        }

        [Serializable]
        public class RegistroRelacion
        {
            public int ProveedorId { get; set; }
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; }
        }

        private GestorRelacionProveedorProducto()
        {
            relaciones = new Dictionary<int, List<RelacionProductoProveedor>>();
            CargarRelaciones();
        }

        public static GestorRelacionProveedorProducto ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new GestorRelacionProveedorProducto();
            return instancia;
        }

        public void AsignarProductoAProveedor(int proveedorId, int productoId, string nombreProducto)
        {
            if (!relaciones.ContainsKey(proveedorId))
            {
                relaciones[proveedorId] = new List<RelacionProductoProveedor>();
            }

            if (!relaciones[proveedorId].Any(p => p.ProductoId == productoId))
            {
                relaciones[proveedorId].Add(new RelacionProductoProveedor
                {
                    ProductoId = productoId,
                    NombreProducto = nombreProducto
                });
                GuardarRelaciones();
            }
        }

        public void QuitarProductoDeProveedor(int proveedorId, int productoId)
        {
            if (relaciones.ContainsKey(proveedorId))
            {
                var producto = relaciones[proveedorId].FirstOrDefault(p => p.ProductoId == productoId);
                if (producto != null)
                {
                    relaciones[proveedorId].Remove(producto);
                    GuardarRelaciones();
                }
            }
        }

        public List<Producto> ObtenerProductosDeProveedor(int proveedorId, List<Producto> todosLosProductos)
        {
            if (relaciones.ContainsKey(proveedorId) && relaciones[proveedorId].Count > 0)
            {
                var idsProductos = relaciones[proveedorId].Select(p => p.ProductoId).ToList();
                return todosLosProductos.Where(p => idsProductos.Contains(p.Id)).ToList();
            }
            return new List<Producto>();
        }



        public List<int> ObtenerIdsProductosDeProveedor(int proveedorId)
        {
            if (relaciones.ContainsKey(proveedorId))
            {
                return relaciones[proveedorId].Select(p => p.ProductoId).ToList();
            }
            return new List<int>();
        }

        private void GuardarRelaciones()
        {
            try
            {
                var datos = new DatosSerializables();
                foreach (var kvp in relaciones)
                {
                    foreach (var producto in kvp.Value)
                    {
                        datos.Relaciones.Add(new RegistroRelacion
                        {
                            ProveedorId = kvp.Key,
                            ProductoId = producto.ProductoId,
                            NombreProducto = producto.NombreProducto
                        });
                    }
                }

                XmlSerializer serializer = new XmlSerializer(typeof(DatosSerializables));
                using (TextWriter writer = new StreamWriter(archivoRelaciones))
                {
                    serializer.Serialize(writer, datos);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar relaciones: {ex.Message}");
            }
        }

        private void CargarRelaciones()
        {
            try
            {
                if (File.Exists(archivoRelaciones))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DatosSerializables));
                    using (TextReader reader = new StreamReader(archivoRelaciones))
                    {
                        var datos = (DatosSerializables)serializer.Deserialize(reader);

                        foreach (var relacion in datos.Relaciones)
                        {
                            if (!relaciones.ContainsKey(relacion.ProveedorId))
                            {
                                relaciones[relacion.ProveedorId] = new List<RelacionProductoProveedor>();
                            }

                            if (!relaciones[relacion.ProveedorId].Any(p => p.ProductoId == relacion.ProductoId))
                            {
                                relaciones[relacion.ProveedorId].Add(new RelacionProductoProveedor
                                {
                                    ProductoId = relacion.ProductoId,
                                    NombreProducto = relacion.NombreProducto
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar relaciones: {ex.Message}");
                relaciones = new Dictionary<int, List<RelacionProductoProveedor>>();
            }
        }
    }
}