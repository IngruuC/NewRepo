using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ENTIDADES.SEGURIDAD
{
    public static class ValidadorContraseña
    {
        public static bool ValidarComplejidad(string contraseña, out string mensajeError)
        {
            mensajeError = string.Empty;

            // Verificar longitud mínima
            if (contraseña.Length < 8)
            {
                mensajeError = "La contraseña debe tener al menos 8 caracteres.";
                return false;
            }

            // Verificar mayúsculas
            if (!Regex.IsMatch(contraseña, "[A-Z]"))
            {
                mensajeError = "La contraseña debe contener al menos una letra mayúscula.";
                return false;
            }

            // Verificar minúsculas
            if (!Regex.IsMatch(contraseña, "[a-z]"))
            {
                mensajeError = "La contraseña debe contener al menos una letra minúscula.";
                return false;
            }

            // Verificar números
            if (!Regex.IsMatch(contraseña, "[0-9]"))
            {
                mensajeError = "La contraseña debe contener al menos un número.";
                return false;
            }

            // Verificar caracteres especiales
            if (!Regex.IsMatch(contraseña, "[^a-zA-Z0-9]"))
            {
                mensajeError = "La contraseña debe contener al menos un carácter especial (*, #, $, etc.).";
                return false;
            }

            return true;
        }
    }
}