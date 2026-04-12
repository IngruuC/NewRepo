using ENTIDADES;
using Microsoft.EntityFrameworkCore;
using MODELO;

namespace CONTROLADORA
{
    public class ControladoraCatalogo
    {
        private static ControladoraCatalogo _instancia;
        public static ControladoraCatalogo ObtenerInstancia()
        {
            if (_instancia == null) _instancia = new ControladoraCatalogo();
            return _instancia;
        }

        public List<CatalogoProveedor> ObtenerTodos()
        {
            using var ctx = new Contexto();
            return ctx.CatalogoProveedores
                .Include(c => c.Proveedor)
                .AsNoTracking()
                .ToList();
        }

        public List<CatalogoProveedor> ObtenerPorProveedor(int proveedorId)
        {
            using var ctx = new Contexto();
            return ctx.CatalogoProveedores
                .Where(c => c.ProveedorId == proveedorId)
                .AsNoTracking()
                .ToList();
        }

        public List<CatalogoProveedor> ObtenerPendientes()
        {
            using var ctx = new Contexto();
            return ctx.CatalogoProveedores
                .Include(c => c.Proveedor)
                .Where(c => c.Estado == "Pendiente")
                .AsNoTracking()
                .ToList();
        }

        public void Agregar(CatalogoProveedor item)
        {
            using var ctx = new Contexto();
            item.FechaOferta = DateTime.Now;
            item.Estado = "Pendiente";
            ctx.CatalogoProveedores.Add(item);
            ctx.SaveChanges();
        }

        public void Modificar(CatalogoProveedor item)
        {
            using var ctx = new Contexto();
            var existing = ctx.CatalogoProveedores.Find(item.Id);
            if (existing == null) throw new Exception("Oferta no encontrada.");
            existing.NombreProducto = item.NombreProducto;
            existing.Precio = item.Precio;
            existing.Descripcion = item.Descripcion;
            ctx.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var ctx = new Contexto();
            var item = ctx.CatalogoProveedores.Find(id);
            if (item == null) throw new Exception("Oferta no encontrada.");
            ctx.CatalogoProveedores.Remove(item);
            ctx.SaveChanges();
        }

        public void CambiarEstado(int id, string nuevoEstado)
        {
            using var ctx = new Contexto();
            var item = ctx.CatalogoProveedores.Find(id);
            if (item == null) throw new Exception("Oferta no encontrada.");
            item.Estado = nuevoEstado;
            ctx.SaveChanges();
        }
    }
}