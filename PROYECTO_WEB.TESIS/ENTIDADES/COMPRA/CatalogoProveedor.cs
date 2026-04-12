using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class CatalogoProveedor
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public string NombreProducto { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaOferta { get; set; } = DateTime.Now;

        // Navegación
        public Proveedor Proveedor { get; set; }
    }
}
