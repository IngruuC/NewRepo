using ENTIDADES.SEGURIDAD;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{
    public class Grupo
    {
        public int Id { get; set; }
        public int ComponenteId { get; set; } // Para compatibilidad con sistema de componentes

        [Required]
        [StringLength(50)]
        public string NombreGrupo { get; set; }


        [StringLength(50)]
        public string Codigo { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; } // Alias para NombreGrupo

        public bool Asignado { get; set; } // Para el DataGridView

        public int GrupoId { get; set; } // Para compatibilidad

        // Relación con EstadoGrupo
        public int? EstadoGrupoId { get; set; }
        [ForeignKey("EstadoGrupoId")]
        public virtual EstadoGrupo EstadoGrupo { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; }

        public virtual ICollection<Permiso> Permisos { get; set; }
        public virtual ICollection<Usuario> Usuarios { get; set; }

        //  Relación con Acciones
        public virtual ICollection<Accion> Acciones { get; set; }

        //  Para manejar jerarquía de permisos
        [NotMapped]
        public List<object> Hijos { get; set; }
        public Grupo()
        {
            Permisos = new HashSet<Permiso>();
            Usuarios = new HashSet<Usuario>();
            Acciones = new HashSet<Accion>();
            Hijos = new List<object>();
            // ComponenteId = Id; // Por defecto
            GrupoId = Id; // Por defecto
        }

        public void AgregarHijo(object hijo)
        {
            if (Hijos == null) Hijos = new List<object>();

            if (!Hijos.Contains(hijo))
            {
                Hijos.Add(hijo);

                // Si es una acción, agregarla también a la colección Acciones
                if (hijo is Accion accion && !Acciones.Contains(accion))
                {
                    Acciones.Add(accion);
                }
            }
        }

        // Propiedades de compatibilidad
        [NotMapped]
        public string Nombre_Alias => NombreGrupo;


    }
}