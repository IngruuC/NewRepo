using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ENTIDADES.SEGURIDAD;

namespace ENTIDADES
{
    [Table("AuditoriasSesion")]
    public class AuditoriaSesion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [Required]
        public DateTime FechaIngreso { get; set; }

        public DateTime? FechaSalida { get; set; }

        [StringLength(50)]
        public string DireccionIP { get; set; }

        [StringLength(100)]
        public string Dispositivo { get; set; }

        [StringLength(20)]
        public string TipoSesion { get; set; }

        public bool SesionActiva { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }
    }
}