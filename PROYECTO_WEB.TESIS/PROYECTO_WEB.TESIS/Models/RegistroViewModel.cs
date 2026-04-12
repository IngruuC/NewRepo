using System.ComponentModel.DataAnnotations;

namespace PROYECTO_WEB.TESIS.Models
{
    public class RegistroViewModel
    {
        [Required] public string NombreUsuario { get; set; }
        [Required][DataType(DataType.Password)] public string Contraseña { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required] public string TipoUsuario { get; set; }

        // Cliente
        public string Documento { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }

        // Proveedor
        public string Cuit { get; set; }
        public string RazonSocial { get; set; }
        public string Telefono { get; set; }
        public string DireccionProveedor { get; set; }

        // Para vinculación con existente
        public int? ClienteId { get; set; }
        public int? ProveedorId { get; set; }
    }
}