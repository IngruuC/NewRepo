using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES.SEGURIDAD
{
    public class EstadoUsuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }

        public EstadoUsuario()
        {
            Usuarios = new HashSet<Usuario>();
        }
    }
}