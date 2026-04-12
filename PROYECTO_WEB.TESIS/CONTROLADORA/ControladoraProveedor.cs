using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraProveedor
    {
        private static ControladoraProveedor instancia;
        private Contexto contexto;

        private ControladoraProveedor()
        {
            contexto = new Contexto();
        }

        public static ControladoraProveedor ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraProveedor();
            return instancia;
        }

        public void AgregarProveedor(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.Cuit) || proveedor.Cuit.Length != 11)
                throw new Exception("El CUIT debe tener 11 dígitos.");

            if (contexto.Proveedores.Any(p => p.Cuit == proveedor.Cuit))
                throw new Exception("Ya existe un proveedor con ese CUIT.");

            contexto.Proveedores.Add(proveedor);
            contexto.SaveChanges();
        }

        public void ModificarProveedor(Proveedor proveedor)
        {
            var proveedorExistente = contexto.Proveedores.Find(proveedor.Id);
            if (proveedorExistente == null)
                throw new Exception("Proveedor no encontrado.");

            if (contexto.Proveedores.Any(p => p.Cuit == proveedor.Cuit && p.Id != proveedor.Id))
                throw new Exception("Ya existe otro proveedor con ese CUIT.");

            proveedorExistente.Cuit = proveedor.Cuit;
            proveedorExistente.RazonSocial = proveedor.RazonSocial;
            proveedorExistente.Telefono = proveedor.Telefono;
            proveedorExistente.Email = proveedor.Email;
            proveedorExistente.Direccion = proveedor.Direccion;

            contexto.SaveChanges();
        }

        public void EliminarProveedor(int id)
        {
            var proveedor = contexto.Proveedores.Find(id);
            if (proveedor == null)
                throw new Exception("Proveedor no encontrado.");

            if (contexto.Compras.Any(c => c.ProveedorId == id))
                throw new Exception("No se puede eliminar el proveedor porque tiene compras asociadas.");

            contexto.Proveedores.Remove(proveedor);
            contexto.SaveChanges();
        }

        public List<Proveedor> ObtenerProveedores()
        {
            return contexto.Proveedores
                .Include(p => p.Usuario)
                .ToList();
        }

        public Proveedor ObtenerProveedorPorId(int id)
        {
            return contexto.Proveedores.Find(id);
        }

        public void ActualizarUsuarioEnProveedor(int proveedorId, int usuarioId)
        {
            var proveedor = contexto.Proveedores.Find(proveedorId);
            if (proveedor == null)
                throw new Exception("Proveedor no encontrado");

            proveedor.UsuarioId = usuarioId;
            contexto.SaveChanges();
        }
    }
}