using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Permiso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombrePermiso { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }

        public virtual ICollection<Grupo> Grupos { get; set; }

        public Permiso()
        {
            Grupos = new HashSet<Grupo>();
        }
    }

}