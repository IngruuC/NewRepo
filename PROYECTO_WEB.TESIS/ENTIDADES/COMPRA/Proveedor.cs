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
    public class Proveedor
    {
        public int Id { get; set; }
        public string Cuit { get; set; }
        public string RazonSocial { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public List<Compra> Compras { get; set; }


        public int? UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        public Proveedor()
        {
            Compras = new List<Compra>();

        }

        public string DatosCompletos  //Para mostrar en combobox
        {
            get { return $"{Id} - {RazonSocial}"; }
        }

        public override string ToString()
        {
            return RazonSocial.Trim();
        }

    }
}