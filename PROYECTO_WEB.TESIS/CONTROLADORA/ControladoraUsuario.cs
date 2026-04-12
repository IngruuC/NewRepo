using ENTIDADES;
using ENTIDADES.SEGURIDAD;
using MODELO;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace CONTROLADORA
{
    public class ControladoraUsuario
    {
        private static ControladoraUsuario instancia;
        private readonly Contexto contexto;
        private const string DEFAULT_PASSWORD = "admin123";

        private ControladoraUsuario()
        {
            contexto = new Contexto();
            InicializarAdministrador();
        }

        public static ControladoraUsuario ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraUsuario();
            return instancia;
        }

        private void InicializarAdministrador()
        {
            try
            {
                var grupoAdmin = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Administrador");
                if (grupoAdmin == null)
                {
                    grupoAdmin = new Grupo
                    {
                        NombreGrupo = "Administrador",
                        Descripcion = "Grupo con todos los permisos"
                    };
                    contexto.Grupos.Add(grupoAdmin);
                    contexto.SaveChanges();
                }

                var usuarioAdmin = contexto.Usuarios
                    .Include(u => u.Grupos)
                    .FirstOrDefault(u => u.NombreUsuario == "admin");

                if (usuarioAdmin == null)
                {
                    usuarioAdmin = new Usuario
                    {
                        NombreUsuario = "admin",
                        Contraseña = BCrypt.Net.BCrypt.HashPassword(DEFAULT_PASSWORD),
                        Rol = "Administrador",
                        Estado = true,
                        FechaCreacion = DateTime.Now,
                        IntentosIngreso = 0,
                        Email = "admin@sistema.com",
                        Grupos = new List<Grupo> { grupoAdmin }
                    };
                    contexto.Usuarios.Add(usuarioAdmin);
                    contexto.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inicializando admin: {ex.Message}");
            }
        }

        public class ResultadoLogin
        {
            public bool Exitoso { get; set; }
            public string Mensaje { get; set; }
            public Usuario Usuario { get; set; }
        }

        public ResultadoLogin ValidarCredenciales(string usuario, string contraseña)
        {
            using var ctx = new Contexto();
            try
            {
                var usuarioEncontrado = ctx.Usuarios
                    .Include(u => u.Grupos)
                    .FirstOrDefault(u => u.NombreUsuario == usuario);

                if (usuarioEncontrado == null)
                    return new ResultadoLogin { Exitoso = false, Mensaje = "Usuario no encontrado" };

                if (!usuarioEncontrado.Estado)
                    return new ResultadoLogin { Exitoso = false, Mensaje = "Usuario desactivado" };

                if (!BCrypt.Net.BCrypt.Verify(contraseña, usuarioEncontrado.Contraseña))
                    return new ResultadoLogin { Exitoso = false, Mensaje = "Contraseña incorrecta" };

                if (usuarioEncontrado.Grupos == null || !usuarioEncontrado.Grupos.Any())
                    return new ResultadoLogin { Exitoso = false, Mensaje = "El usuario no tiene grupos asignados" };

                return new ResultadoLogin { Exitoso = true, Usuario = usuarioEncontrado, Mensaje = "Login exitoso" };
            }
            catch (Exception ex)
            {
                return new ResultadoLogin { Exitoso = false, Mensaje = $"Error: {ex.Message}" };
            }
        }

        public void ActualizarAcceso(int usuarioId)
        {
            using var ctx = new Contexto();
            var usuario = ctx.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                usuario.UltimoAcceso = DateTime.Now;
                usuario.IntentosIngreso = 0;
                ctx.SaveChanges();
            }
        }

        public List<Usuario> ObtenerUsuarios()
        {
            using var ctx = new Contexto();
            try
            {
                var usuarios = ctx.Usuarios
                    .Include(u => u.Grupos)
                    .AsNoTracking()
                    .ToList();

                foreach (var u in usuarios)
                {
                    u.Email ??= string.Empty;
                    u.NombreyApellido ??= string.Empty;
                    u.Clave ??= string.Empty;
                    u.Rol ??= string.Empty;
                    u.Grupos ??= new List<Grupo>();
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener usuarios: {ex.Message}");
                return new List<Usuario>();
            }
        }

        public Usuario ObtenerUsuario(string nombreUsuario)
        {
            using var ctx = new Contexto();
            return ctx.Usuarios
                .Include(u => u.Grupos)
                .FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
        }

        public Usuario ObtenerUsuarioPorId(int id)
        {
            using var ctx = new Contexto();
            return ctx.Usuarios
                .Include(u => u.Grupos)
                .FirstOrDefault(u => u.Id == id);
        }

        public List<Cliente> ObtenerClientes()
        {
            using var ctx = new Contexto();
            return ctx.Clientes
                .Include(c => c.Usuario)
                .AsNoTracking()
                .ToList();
        }

        public void AgregarUsuario(Usuario usuario)
        {
            if (contexto.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                throw new Exception("Ya existe un usuario con ese nombre");

            if (!string.IsNullOrEmpty(usuario.Email) &&
                contexto.Usuarios.Any(u => u.Email == usuario.Email))
                throw new Exception("Ya existe un usuario con ese email");

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
        }

        public void ModificarUsuario(Usuario usuario)
        {
            var usuarioExistente = contexto.Usuarios.Find(usuario.Id);
            if (usuarioExistente == null)
                throw new Exception("Usuario no encontrado");

            usuarioExistente.Email = usuario.Email;
            usuarioExistente.Estado = usuario.Estado;
            contexto.SaveChanges();
        }

        public void EliminarUsuario(int id)
        {
            var usuario = contexto.Usuarios.Find(id);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            if (usuario.NombreUsuario == "admin")
                throw new Exception("No se puede eliminar el usuario administrador");

            contexto.Usuarios.Remove(usuario);
            contexto.SaveChanges();
        }

        public void CambiarContrasena(int usuarioId, string nuevaContrasena)
        {
            var usuario = contexto.Usuarios.Find(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            string mensajeError;
            if (!ValidadorContraseña.ValidarComplejidad(nuevaContrasena, out mensajeError))
                throw new Exception(mensajeError);

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena);
            usuario.IntentosIngreso = 0;
            contexto.SaveChanges();
        }

        public void RegistrarCliente(Usuario usuario, Cliente cliente)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                if (contexto.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                    throw new Exception("El nombre de usuario ya está en uso");

                if (contexto.Clientes.Any(c => c.Documento == cliente.Documento))
                    throw new Exception("Ya existe un cliente con ese documento");

                string mensajeError;
                if (!ValidadorContraseña.ValidarComplejidad(usuario.Contraseña, out mensajeError))
                    throw new Exception(mensajeError);

                var grupoCliente = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Cliente");
                if (grupoCliente == null)
                    throw new Exception("No se encontró el grupo Cliente");

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
                usuario.Grupos = new List<Grupo> { grupoCliente };
                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                cliente.UsuarioId = usuario.Id;
                contexto.Clientes.Add(cliente);
                contexto.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al registrar cliente: {ex.Message} | Inner: {ex.InnerException?.Message} | Inner2: {ex.InnerException?.InnerException?.Message}");
            }
        }

        public void VincularUsuarioAClienteExistente(Usuario usuario, int clienteId)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var cliente = contexto.Clientes.Find(clienteId);
                if (cliente == null)
                    throw new Exception($"Cliente con ID {clienteId} no encontrado.");

                if (cliente.UsuarioId.HasValue)
                    throw new Exception($"Este cliente ya tiene un usuario asignado (UsuarioId: {cliente.UsuarioId}).");

                if (contexto.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                    throw new Exception($"El nombre de usuario '{usuario.NombreUsuario}' ya está en uso.");

                if (!string.IsNullOrEmpty(usuario.Email) &&
                    contexto.Usuarios.Any(u => u.Email == usuario.Email))
                    throw new Exception($"Ya existe una cuenta con el email '{usuario.Email}'.");

                string mensajeError;
                if (!ValidadorContraseña.ValidarComplejidad(usuario.Contraseña, out mensajeError))
                    throw new Exception(mensajeError);

                var grupoCliente = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Cliente");
                if (grupoCliente == null)
                    throw new Exception("No se encontró el grupo 'Cliente' en la base de datos. Debe crearlo primero desde Seguridad > Grupos.");

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
                usuario.Rol = "Cliente";
                usuario.Estado = true;
                usuario.FechaCreacion = DateTime.Now;
                usuario.IntentosIngreso = 0;
                usuario.Grupos = new List<Grupo> { grupoCliente };

                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                // Refrescar el cliente desde la BD para evitar problemas de tracking
                var clienteDb = contexto.Clientes.Find(clienteId);
                if (clienteDb == null)
                    throw new Exception($"No se pudo refrescar el cliente con ID {clienteId}.");

                clienteDb.UsuarioId = usuario.Id;
                contexto.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al vincular usuario: {ex.Message} | Inner: {ex.InnerException?.Message} | Inner2: {ex.InnerException?.InnerException?.Message}");
            }
        }

        public void AsignarUsuarioDefaultACliente(int clienteId)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var cliente = contexto.Clientes.Find(clienteId);
                if (cliente == null)
                    throw new Exception("Cliente no encontrado.");

                if (cliente.UsuarioId.HasValue)
                    throw new Exception("Este cliente ya tiene un usuario asignado.");

                var nombreUsuario = $"cliente_{cliente.Documento}";

                if (contexto.Usuarios.Any(u => u.NombreUsuario == nombreUsuario))
                    throw new Exception($"Ya existe el usuario {nombreUsuario}.");

                var grupoCliente = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Cliente");
                if (grupoCliente == null)
                    throw new Exception("No se encontró el grupo Cliente.");

                var passwordDefault = $"Cliente_{cliente.Documento}!";

                var usuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Contraseña = BCrypt.Net.BCrypt.HashPassword(passwordDefault),
                    Rol = "Cliente",
                    Estado = true,
                    FechaCreacion = DateTime.Now,
                    IntentosIngreso = 0,
                    Email = "",
                    Grupos = new List<Grupo> { grupoCliente }
                };

                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                cliente.UsuarioId = usuario.Id;
                contexto.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al asignar usuario: {ex.Message}");
            }
        }

        public void RegistrarProveedor(Usuario usuario, Proveedor proveedor)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                if (contexto.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                    throw new Exception("El nombre de usuario ya está en uso");

                if (contexto.Proveedores.Any(p => p.Cuit == proveedor.Cuit))
                    throw new Exception("Ya existe un proveedor con ese CUIT");

                string mensajeError;
                if (!ValidadorContraseña.ValidarComplejidad(usuario.Contraseña, out mensajeError))
                    throw new Exception(mensajeError);

                var grupoProveedor = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Proveedor");
                if (grupoProveedor == null)
                    throw new Exception("No se encontró el grupo Proveedor");

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
                usuario.Grupos = new List<Grupo> { grupoProveedor };
                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                proveedor.UsuarioId = usuario.Id;
                contexto.Proveedores.Add(proveedor);
                contexto.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void VincularUsuarioAProveedorExistente(Usuario usuario, int proveedorId)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var proveedor = contexto.Proveedores.Find(proveedorId);
                if (proveedor == null)
                    throw new Exception("Proveedor no encontrado.");

                if (proveedor.UsuarioId.HasValue)
                    throw new Exception("Este proveedor ya tiene un usuario asignado.");

                if (contexto.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                    throw new Exception("El nombre de usuario ya está en uso.");

                string mensajeError;
                if (!ValidadorContraseña.ValidarComplejidad(usuario.Contraseña, out mensajeError))
                    throw new Exception(mensajeError);

                var grupoProveedor = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Proveedor");
                if (grupoProveedor == null)
                    throw new Exception("No se encontró el grupo Proveedor.");

                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
                usuario.Rol = "Proveedor";
                usuario.Estado = true;
                usuario.FechaCreacion = DateTime.Now;
                usuario.IntentosIngreso = 0;
                usuario.Grupos = new List<Grupo> { grupoProveedor };

                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                proveedor.UsuarioId = usuario.Id;
                contexto.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al vincular usuario: {ex.Message}");
            }
        }

        public void AsignarUsuarioDefaultAProveedor(int proveedorId)
        {
            using var transaction = contexto.Database.BeginTransaction();
            try
            {
                var proveedor = contexto.Proveedores.Find(proveedorId);
                if (proveedor == null)
                    throw new Exception("Proveedor no encontrado.");

                if (proveedor.UsuarioId.HasValue)
                    throw new Exception("Este proveedor ya tiene un usuario asignado.");

                var nombreUsuario = $"proveedor_{proveedor.Cuit}";

                if (contexto.Usuarios.Any(u => u.NombreUsuario == nombreUsuario))
                    throw new Exception($"Ya existe el usuario {nombreUsuario}.");

                var grupoProveedor = contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == "Proveedor");
                if (grupoProveedor == null)
                    throw new Exception("No se encontró el grupo Proveedor.");

                var passwordDefault = $"Proveedor_{proveedor.Cuit}!";

                var usuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Contraseña = BCrypt.Net.BCrypt.HashPassword(passwordDefault),
                    Rol = "Proveedor",
                    Estado = true,
                    FechaCreacion = DateTime.Now,
                    IntentosIngreso = 0,
                    Email = proveedor.Email ?? "",
                    Grupos = new List<Grupo> { grupoProveedor }
                };

                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();

                proveedor.UsuarioId = usuario.Id;
                contexto.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error al asignar usuario: {ex.Message}");
            }
        }

        public bool ExisteUsuario(string nombreUsuario)
            => contexto.Usuarios.Any(u => u.NombreUsuario == nombreUsuario);

        public bool ExisteCliente(string documento)
            => contexto.Clientes.Any(c => c.Documento == documento);

        public bool ExisteProveedor(string cuit)
            => contexto.Proveedores.Any(p => p.Cuit == cuit);

        public Grupo ObtenerGrupoPorNombre(string nombreGrupo)
            => contexto.Grupos.FirstOrDefault(g => g.NombreGrupo == nombreGrupo);

        public void AgregarUsuarioConGrupo(Usuario usuario, int grupoId)
        {
            var grupo = contexto.Grupos.Find(grupoId);
            if (grupo == null)
                throw new Exception($"Grupo con ID {grupoId} no encontrado");

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            usuario.Grupos = new List<Grupo> { grupo };
            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
        }

        public List<Grupo> ObtenerGrupos()
            => contexto.Grupos.ToList();

        public bool SolicitarRecuperacionClave(string nombreUsuario, string emailRegistrado, string resendApiKey)
        {
            try
            {
                var usuario = contexto.Usuarios.FirstOrDefault(u =>
                    u.NombreUsuario == nombreUsuario &&
                    u.Email == emailRegistrado &&
                    u.Estado == true);

                if (usuario == null) return false;

                string nuevaContraseña = GenerarContraseñaSegura();
                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña);
                usuario.IntentosIngreso = 0;
                contexto.SaveChanges();

                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {resendApiKey}");

                string cuerpo = $@"
            <div style='font-family:Segoe UI,sans-serif;max-width:500px;margin:auto;
                border:2px solid #d4a017;border-radius:10px;overflow:hidden;'>
                <div style='background:linear-gradient(135deg,#b8860b,#d4a017);
                    padding:20px;text-align:center;'>
                    <h2 style='color:#fff;margin:0;letter-spacing:1px;'>
                        🛒 Fresco Market
                    </h2>
                    <p style='color:#fff8dc;margin:4px 0 0;font-size:12px;'>
                        Sistema de Gestión de Minimarket
                    </p>
                </div>
                <div style='padding:28px;background:#fffbe6;'>
                    <p style='color:#3a2500;font-size:14px;'>
                        Hola <strong>{usuario.NombreUsuario}</strong>,
                    </p>
                    <p style='color:#5a3e00;font-size:13px;'>
                        Tu nueva contraseña temporal es:
                    </p>
                    <div style='background:#fff;border:2px solid #d4a017;border-radius:8px;
                        padding:16px;text-align:center;margin:20px 0;'>
                        <span style='font-size:22px;font-weight:800;
                            color:#b8860b;letter-spacing:3px;font-family:monospace;'>
                            {nuevaContraseña}
                        </span>
                    </div>
                    <p style='color:#c0392b;font-size:12px;font-weight:600;'>
                        ⚠️ Cambiá esta contraseña desde tu perfil al ingresar.
                    </p>
                </div>
            </div>";

                var payload = new
                {
                    from = "Fresco Market <noreply@frescomarket.com>",
                    to = new[] { emailRegistrado },
                    subject = "🔑 Fresco Market — Nueva contraseña temporal",
                    html = cuerpo
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync("https://api.resend.com/emails", content).Result;
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recuperar clave: {ex.Message}");
                return false;
            }
        }

        private string GenerarContraseñaSegura()
        {
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string especiales = "!@#$%^&*()_-+=<>?";

            var random = new Random();
            var chars = new char[12];

            chars[0] = mayusculas[random.Next(mayusculas.Length)];
            chars[1] = minusculas[random.Next(minusculas.Length)];
            chars[2] = numeros[random.Next(numeros.Length)];
            chars[3] = especiales[random.Next(especiales.Length)];

            string todos = mayusculas + minusculas + numeros + especiales;
            for (int i = 4; i < chars.Length; i++)
                chars[i] = todos[random.Next(todos.Length)];

            for (int i = 0; i < chars.Length; i++)
            {
                int j = random.Next(chars.Length);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}