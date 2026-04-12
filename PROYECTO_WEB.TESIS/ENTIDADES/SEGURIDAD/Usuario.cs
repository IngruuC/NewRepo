using ENTIDADES.SEGURIDAD;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace ENTIDADES
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        public string Contraseña { get; set; }

        [Required]
        public string Rol { get; set; }

        [Required]
        public bool Estado { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        public DateTime? UltimoAcceso { get; set; }

        [Required]
        public int IntentosIngreso { get; set; }

        public string? Email { get; set; }  // antes string

        public virtual ICollection<Grupo> Grupos { get; set; }

        [StringLength(100)]
        public string? NombreyApellido { get; set; }  // antes string

        public string? Clave { get; set; }  // antes string

        public int? EstadoUsuarioId { get; set; }  // ya es nullable, perfecto
        [ForeignKey("EstadoUsuarioId")]
        public virtual EstadoUsuario? EstadoUsuario { get; set; }  // <-- agregar ?

        [NotMapped]
        public List<object> Perfil { get; set; }

        public Usuario()
        {
            Estado = true;
            FechaCreacion = DateTime.Now;
            IntentosIngreso = 0;
            Grupos = new HashSet<Grupo>();
            Perfil = new List<object>();
        }

        public void AgregarPermiso(object permiso)
        {
            if (Perfil == null) Perfil = new List<object>();
            if (!Perfil.Contains(permiso))
            {
                Perfil.Add(permiso);
                if (permiso is Grupo grupo)
                {
                    if (!Grupos.Contains(grupo))
                        Grupos.Add(grupo);
                }
            }
        }

        public void EliminarPermiso(object permiso)
        {
            if (Perfil != null)
            {
                Perfil.Remove(permiso);
                if (permiso is Grupo grupo)
                    Grupos.Remove(grupo);
            }
        }

        [NotMapped]
        public string Clave_Alias => Contraseña;
    }
}