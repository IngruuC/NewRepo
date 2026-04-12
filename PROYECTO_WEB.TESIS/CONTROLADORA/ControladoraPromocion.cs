using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraPromocion
    {
        private static ControladoraPromocion _instancia;
        public static ControladoraPromocion ObtenerInstancia()
        {
            if (_instancia == null) _instancia = new ControladoraPromocion();
            return _instancia;
        }

        public List<Promocion> ObtenerTodas()
        {
            using var ctx = new Contexto();
            return ctx.Promociones
                .Include(p => p.Producto)
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToList();
        }

        public List<Promocion> ObtenerVigentes()
        {
            using var ctx = new Contexto();
            var ahora = DateTime.Now;
            return ctx.Promociones
                .Include(p => p.Producto)
                .Where(p => p.Activa && p.FechaInicio <= ahora && p.FechaFin >= ahora)
                .AsNoTracking()
                .ToList();
        }

        public Promocion ObtenerVigentePorProducto(int productoId)
        {
            using var ctx = new Contexto();
            var ahora = DateTime.Now;
            return ctx.Promociones
                .Where(p => p.ProductoId == productoId
                         && p.Activa
                         && p.FechaInicio <= ahora
                         && p.FechaFin >= ahora)
                .AsNoTracking()
                .FirstOrDefault();
        }

        public void Agregar(Promocion promocion)
        {
            using var ctx = new Contexto();
            promocion.FechaInicio = DateTime.Now;
            promocion.Activa = true;
            ctx.Promociones.Add(promocion);
            ctx.SaveChanges();
        }

        public void Desactivar(int id)
        {
            using var ctx = new Contexto();
            var promo = ctx.Promociones.Find(id);
            if (promo == null) throw new Exception("Promoción no encontrada.");
            promo.Activa = false;
            ctx.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var ctx = new Contexto();
            var promo = ctx.Promociones.Find(id);
            if (promo == null) throw new Exception("Promoción no encontrada.");
            ctx.Promociones.Remove(promo);
            ctx.SaveChanges();
        }
    }
}