using ENTIDADES;
using ENTIDADES.SEGURIDAD;
using MODELO;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CONTROLADORA
{
    public class ControladoraSeguridad
    {
        private static ControladoraSeguridad _instancia;
        private readonly Contexto contexto;

        private ControladoraSeguridad()
        {
            contexto = new Contexto();
            InicializarEstados();
        }

        public static ControladoraSeguridad Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new ControladoraSeguridad();
                return _instancia;
            }
        }

        private void InicializarEstados()
        {
            try
            {
                // Crear estados de usuario básicos
                if (!contexto.EstadosUsuario.Any())
                {
                    var estadosUsuario = new[]
                    {
                        new EstadoUsuario { Nombre = "Activo", Descripcion = "Usuario activo en el sistema" },
                        new EstadoUsuario { Nombre = "Inactivo", Descripcion = "Usuario inactivo temporalmente" },
                        new EstadoUsuario { Nombre = "Bloqueado", Descripcion = "Usuario bloqueado por seguridad" }
                    };
                    contexto.EstadosUsuario.AddRange(estadosUsuario);
                }

                // Crear estados de grupo básicos
                if (!contexto.EstadosGrupo.Any())
                {
                    var estadosGrupo = new[]
                    {
                        new EstadoGrupo { Nombre = "Activo", Descripcion = "Grupo activo en el sistema" },
                        new EstadoGrupo { Nombre = "Inactivo", Descripcion = "Grupo inactivo temporalmente" }
                    };
                    contexto.EstadosGrupo.AddRange(estadosGrupo);
                }

                // Crear acciones básicas del sistema
                if (!contexto.Acciones.Any())
                {
                    var acciones = new[]
                    {
                        new Accion { ComponenteId = 1, Codigo = "VENTA_CREAR", Nombre = "Crear Venta", Descripcion = "Permite crear nuevas ventas" },
                        new Accion { ComponenteId = 2, Codigo = "VENTA_VER", Nombre = "Ver Ventas", Descripcion = "Permite ver el listado de ventas" },
                        new Accion { ComponenteId = 3, Codigo = "PRODUCTO_CREAR", Nombre = "Crear Producto", Descripcion = "Permite crear nuevos productos" },
                        new Accion { ComponenteId = 4, Codigo = "PRODUCTO_MODIFICAR", Nombre = "Modificar Producto", Descripcion = "Permite modificar productos existentes" },
                        new Accion { ComponenteId = 5, Codigo = "CLIENTE_CREAR", Nombre = "Crear Cliente", Descripcion = "Permite registrar nuevos clientes" },
                        new Accion { ComponenteId = 6, Codigo = "CLIENTE_VER", Nombre = "Ver Clientes", Descripcion = "Permite ver el listado de clientes" },
                        new Accion { ComponenteId = 7, Codigo = "COMPRA_CREAR", Nombre = "Crear Compra", Descripcion = "Permite crear nuevas compras" },
                        new Accion { ComponenteId = 8, Codigo = "COMPRA_VER", Nombre = "Ver Compras", Descripcion = "Permite ver el listado de compras" },
                        new Accion { ComponenteId = 9, Codigo = "USUARIO_GESTIONAR", Nombre = "Gestionar Usuarios", Descripcion = "Permite gestionar usuarios del sistema" },
                        new Accion { ComponenteId = 10, Codigo = "GRUPO_GESTIONAR", Nombre = "Gestionar Grupos", Descripcion = "Permite gestionar grupos y permisos" },
                        new Accion { ComponenteId = 11, Codigo = "AUDITORIA_VER", Nombre = "Ver Auditoría", Descripcion = "Permite ver registros de auditoría" },
                        new Accion { ComponenteId = 12, Codigo = "BACKUP_GESTIONAR", Nombre = "Gestionar Backup", Descripcion = "Permite realizar backup y restauración" }
                    };
                    contexto.Acciones.AddRange(acciones);
                }

                contexto.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando datos de seguridad: {ex.Message}");
            }
        }

        #region Gestión de Usuarios
        public List<Usuario> RecuperarUsuarios()
        {
            try
            {
                return contexto.Usuarios
                    .Include(u => u.EstadoUsuario)
                    .Include(u => u.Grupos)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al recuperar usuarios: {ex.Message}");
                return new List<Usuario>();
            }
        }


        public string AgregarUsuario(Usuario usuario)
        {
            try
            {
                // Validar que no exista el usuario
                if (contexto.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                    return "Ya existe un usuario con ese nombre";

                // Hashear contraseña si no está hasheada
                if (!string.IsNullOrEmpty(usuario.Contraseña) && !usuario.Contraseña.StartsWith("$2"))
                {
                    usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
                }

                usuario.FechaCreacion = DateTime.Now;
                usuario.Estado = true;

                contexto.Usuarios.Add(usuario);
                contexto.SaveChanges();
                return "Usuario agregado exitosamente";
            }
            catch (Exception ex)
            {
                return $"Error al agregar usuario: {ex.Message}";
            }
        }

        public string ModificarUsuario(Usuario usuario)
        {
            try
            {
                using var ctx = new Contexto();
                var usuarioExistente = ctx.Usuarios.Find(usuario.Id);
                if (usuarioExistente == null)
                    return "Usuario no encontrado";

                usuarioExistente.Email = usuario.Email;
                usuarioExistente.Estado = usuario.Estado;

                ctx.SaveChanges();
                return "Usuario modificado exitosamente";
            }
            catch (Exception ex)
            {
                return $"Error al modificar usuario: {ex.Message}";
            }
        }

        public string EliminarUsuario(Usuario usuario)
        {
            try
            {
                if (usuario.NombreUsuario == "admin")
                    return "No se puede eliminar el usuario administrador";

                var usuarioExistente = contexto.Usuarios.Find(usuario.Id);
                if (usuarioExistente == null)
                    return "Usuario no encontrado";

                contexto.Usuarios.Remove(usuarioExistente);
                contexto.SaveChanges();
                return "Usuario eliminado exitosamente";
            }
            catch (Exception ex)
            {
                return $"Error al eliminar usuario: {ex.Message}";
            }
        }

        public string ResetearClave(Usuario usuario)
        {
            try
            {
                var usuarioExistente = contexto.Usuarios.Find(usuario.Id);
                if (usuarioExistente == null)
                    return "Usuario no encontrado";

                string nuevaClave = GenerarClaveAleatoria();
                usuarioExistente.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
                usuarioExistente.IntentosIngreso = 0;

                contexto.SaveChanges();
                return $"Clave reseteada exitosamente. Nueva clave: {nuevaClave}";
            }
            catch (Exception ex)
            {
                return $"Error al resetear clave: {ex.Message}";
            }
        }
        #endregion

        #region Gestión de Grupos
        public List<Grupo> RecuperarGrupos()
        {
            try
            {
                return contexto.Grupos
                    .Include(g => g.EstadoGrupo)
                    .Include(g => g.Acciones)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al recuperar grupos: {ex.Message}");
                return new List<Grupo>();
            }
        }

        public string AgregarGrupo(Grupo grupo)
        {
            try
            {
                if (contexto.Grupos.Any(g => g.NombreGrupo == grupo.NombreGrupo))
                    return "Ya existe un grupo con ese nombre";

                contexto.Grupos.Add(grupo);
                contexto.SaveChanges();
                return "Grupo agregado exitosamente";
            }
            catch (Exception ex)
            {
                return $"Error al agregar grupo: {ex.Message}";
            }
        }

        public string ModificarGrupo(Grupo grupo)
        {
            try
            {
                var grupoExistente = contexto.Grupos.Find(grupo.Id);
                if (grupoExistente == null)
                    return "Grupo no encontrado";

                grupoExistente.Codigo = grupo.Codigo;
                grupoExistente.NombreGrupo = grupo.NombreGrupo;
                grupoExistente.Descripcion = grupo.Descripcion;
                grupoExistente.EstadoGrupoId = grupo.EstadoGrupoId;
                grupoExistente.Acciones = grupo.Acciones;

                contexto.SaveChanges();
                return "Grupo modificado exitosamente";
            }
            catch (Exception ex)
            {
                return $"Error al modificar grupo: {ex.Message}";
            }
        }

        public string EliminarGrupo(Grupo grupo)
        {
            try
            {
                var grupoExistente = contexto.Grupos.Find(grupo.Id);
                if (grupoExistente == null)
                    return "Grupo no encontrado";

                if (grupoExistente.Usuarios.Any())
                    return "No se puede eliminar un grupo que tiene usuarios asignados";

                contexto.Grupos.Remove(grupoExistente);
                contexto.SaveChanges();
                return "Grupo eliminado exitosamente";
            }
            catch (Exception ex)
            {
                return $"Error al eliminar grupo: {ex.Message}";
            }
        }
        #endregion

        #region Métodos auxiliares
        public List<EstadoUsuario> RecuperarEstadosUsuario()
        {
            return contexto.EstadosUsuario.ToList();
        }

        public List<EstadoGrupo> RecuperarEstadosGrupo()
        {
            return contexto.EstadosGrupo.ToList();
        }

        public List<Accion> RecuperarAcciones()
        {
            return contexto.Acciones.ToList();
        }

        private string GenerarClaveAleatoria()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion





    }
}