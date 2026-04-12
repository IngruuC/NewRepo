using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Favorito
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ProductoId { get; set; }
        public DateTime FechaAgregado { get; set; } = DateTime.Now;

        // Navegación
        public Cliente Cliente { get; set; }
        public Producto Producto { get; set; }
    }
}