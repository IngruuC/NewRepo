using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTIDADES
{

    public class Cliente
    {
        public int Id { get; set; }
        public string Documento { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public List<Venta> Ventas { get; set; }


        // Nuevas propiedades para la relación con Usuario
        public int? UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }


        public Cliente()
        {
            Ventas = new List<Venta>();
        }

        public string DatosCompletos  //Es para mostrar los datos en el combobox del formVenta
        {
            get { return $"{Id} - {Nombre} {Apellido}"; }
        }
        public override string ToString()
        {
            return $"{Nombre} {Apellido}".Trim();
        }
    }
}