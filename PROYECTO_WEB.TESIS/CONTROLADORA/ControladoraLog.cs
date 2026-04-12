using ENTIDADES;
using MODELO;

namespace CONTROLADORA
{
    public class ControladoraLog
    {
        private static ControladoraLog? _instancia;
        public static ControladoraLog ObtenerInstancia()
        {
            _instancia ??= new ControladoraLog();
            return _instancia;
        }

        public void Registrar(string usuarioNombre, string accion, string? detalle = null, string? ip = null)
        {
            try
            {
                using var ctx = new Contexto();
                ctx.LogActividad.Add(new LogActividad
                {
                    UsuarioNombre = usuarioNombre,
                    Accion = accion,
                    Detalle = detalle,
                    Fecha = DateTime.Now,
                    IP = ip
                });
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR LogActividad: {ex.Message}");
            }
        }

        public List<LogActividad> ObtenerTodos()
        {
            try
            {
                using var ctx = new Contexto();
                return ctx.LogActividad
                    .OrderByDescending(l => l.Fecha)
                    .Take(200)
                    .ToList();
            }
            catch
            {
                return new List<LogActividad>();
            }
        }

        public List<LogActividad> ObtenerRecientes(int cantidad = 50)
        {
            try
            {
                using var ctx = new Contexto();
                return ctx.LogActividad
                    .OrderByDescending(l => l.Fecha)
                    .Take(cantidad)
                    .ToList();
            }
            catch
            {
                return new List<LogActividad>();
            }
        }
    }
}