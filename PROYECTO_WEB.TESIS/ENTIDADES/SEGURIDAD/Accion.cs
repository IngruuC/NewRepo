using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ENTIDADES.SEGURIDAD
{
    public class Accion
    {
        public int Id { get; set; }
        public int ComponenteId { get; set; }

        [Required]
        [StringLength(100)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }

        public bool Asignada { get; set; } // Para el DataGridView

        public virtual ICollection<Grupo> Grupos { get; set; }

        public Accion()
        {
            Grupos = new HashSet<Grupo>();
        }
    }
}