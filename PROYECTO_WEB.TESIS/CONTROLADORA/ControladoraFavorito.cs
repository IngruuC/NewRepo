using ENTIDADES;
using MODELO;
using Microsoft.EntityFrameworkCore;

namespace CONTROLADORA
{
    public class ControladoraFavorito
    {
        private static ControladoraFavorito _instancia;
        public static ControladoraFavorito ObtenerInstancia()
        {
            if (_instancia == null) _instancia = new ControladoraFavorito();
            return _instancia;
        }

        public List<Favorito> ObtenerPorCliente(int clienteId)
        {
            using var ctx = new Contexto();
            return ctx.Favoritos
                .Include(f => f.Producto)
                .Where(f => f.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }

        public bool EsFavorito(int clienteId, int productoId)
        {
            using var ctx = new Contexto();
            return ctx.Favoritos.Any(f => f.ClienteId == clienteId && f.ProductoId == productoId);
        }

        public void Agregar(int clienteId, int productoId)
        {
            using var ctx = new Contexto();
            if (ctx.Favoritos.Any(f => f.ClienteId == clienteId && f.ProductoId == productoId))
                return; // Ya existe
            ctx.Favoritos.Add(new Favorito
            {
                ClienteId = clienteId,
                ProductoId = productoId,
                FechaAgregado = DateTime.Now
            });
            ctx.SaveChanges();
        }

        public void Eliminar(int clienteId, int productoId)
        {
            using var ctx = new Contexto();
            var fav = ctx.Favoritos
                .FirstOrDefault(f => f.ClienteId == clienteId && f.ProductoId == productoId);
            if (fav != null)
            {
                ctx.Favoritos.Remove(fav);
                ctx.SaveChanges();
            }
        }
    }
}