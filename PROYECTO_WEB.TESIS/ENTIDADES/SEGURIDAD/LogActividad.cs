using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class LogActividad
    {
        public int Id { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string? IP { get; set; }
    }
}