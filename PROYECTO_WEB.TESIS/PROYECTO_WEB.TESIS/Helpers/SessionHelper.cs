using ENTIDADES;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PROYECTO_WEB.TESIS.Helpers
{
    public static class SessionHelper
    {
        private const string KEY_USUARIO_ID = "UsuarioId";
        private const string KEY_USUARIO_NOMBRE = "UsuarioNombre";
        private const string KEY_USUARIO_ROL = "UsuarioRol";
        private const string KEY_USUARIO_GRUPOS = "UsuarioGrupos";

        public static void SetUsuario(ISession session, Usuario usuario)
        {
            session.SetInt32(KEY_USUARIO_ID, usuario.Id);
            session.SetString(KEY_USUARIO_NOMBRE, usuario.NombreUsuario);
            session.SetString(KEY_USUARIO_ROL, usuario.Rol ?? "");
            var grupos = usuario.Grupos?.Select(g => g.NombreGrupo).ToList() ?? new List<string>();
            session.SetString(KEY_USUARIO_GRUPOS, JsonSerializer.Serialize(grupos));
        }

        public static int? GetUsuarioId(ISession session)
            => session.GetInt32(KEY_USUARIO_ID);

        public static string GetUsuarioNombre(ISession session)
            => session.GetString(KEY_USUARIO_NOMBRE);

        public static string GetUsuarioRol(ISession session)
            => session.GetString(KEY_USUARIO_ROL);

        public static List<string> GetUsuarioGrupos(ISession session)
        {
            var json = session.GetString(KEY_USUARIO_GRUPOS);
            if (string.IsNullOrEmpty(json)) return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        public static void SetRequiereCambio(ISession session, bool requiere)
    => session.SetString("RequiereCambioContrasena", requiere.ToString());

        public static bool GetRequiereCambio(ISession session)
            => session.GetString("RequiereCambioContrasena") == "True";

        public static bool EsAdministrador(ISession session)
        {
            var grupos = GetUsuarioGrupos(session);
            return grupos.Any(g => g.ToUpper() == "ADMINISTRADOR");
        }

        public static bool EsProveedor(ISession session)
        {
            var grupos = GetUsuarioGrupos(session);
            return grupos.Any(g => g.ToUpper() == "PROVEEDOR");
        }

        public static bool EsCliente(ISession session)
        {
            var grupos = GetUsuarioGrupos(session);
            return grupos.Any(g => g.ToUpper() == "CLIENTE");
        }

        public static bool EstaLogueado(ISession session)
            => session.GetInt32(KEY_USUARIO_ID).HasValue;

        public static void CerrarSesion(ISession session)
            => session.Clear();
    }
}