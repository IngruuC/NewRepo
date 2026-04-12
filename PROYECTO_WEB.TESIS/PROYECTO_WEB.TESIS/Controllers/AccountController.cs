using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;
using PROYECTO_WEB.TESIS.Models;

namespace PROYECTO_WEB.TESIS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ControladoraUsuario _controladoraUsuario;
        private readonly ControladoraAuditoria _controladoraAuditoria;
        private readonly ControladoraProveedor _controladoraProveedor;
        private readonly IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _controladoraUsuario = ControladoraUsuario.ObtenerInstancia();
            _controladoraAuditoria = new ControladoraAuditoria();
            _controladoraProveedor = ControladoraProveedor.ObtenerInstancia();
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (SessionHelper.EstaLogueado(HttpContext.Session)) return RedirectToRol();
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var resultado = _controladoraUsuario.ValidarCredenciales(model.Usuario, model.Contraseña);
            if (!resultado.Exitoso) { ModelState.AddModelError("", resultado.Mensaje); return View(model); }
            SessionHelper.SetUsuario(HttpContext.Session, resultado.Usuario);
            _controladoraUsuario.ActualizarAcceso(resultado.Usuario.Id);
            string ip = ObtenerIPReal();
            _controladoraAuditoria.RegistrarInicioSesion(resultado.Usuario, ip);

            // ── Marcar si requiere cambio de contraseña ──
            var nombreUsuario = resultado.Usuario.NombreUsuario;
            var requiereCambio = nombreUsuario.StartsWith("cliente_") ||
                                 nombreUsuario.StartsWith("proveedor_");
            SessionHelper.SetRequiereCambio(HttpContext.Session, requiereCambio);

            return RedirectToRol();
        }

        [HttpPost]
        public JsonResult CambiarContrasenaSession([FromBody] CambiarContrasenaRequest request)
        {
            try
            {
                var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
                if (!usuarioId.HasValue)
                    return Json(new { ok = false, error = "Sesión expirada." });

                _controladoraUsuario.CambiarContrasena(usuarioId.Value, request.NuevaContrasena);
                SessionHelper.SetRequiereCambio(HttpContext.Session, false);

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        public class CambiarContrasenaRequest
        {
            public string NuevaContrasena { get; set; }
        }

        public IActionResult Logout()
        {
            var usuarioId = SessionHelper.GetUsuarioId(HttpContext.Session);
            if (usuarioId.HasValue) _controladoraAuditoria.RegistrarCierreSesion(usuarioId.Value);
            SessionHelper.CerrarSesion(HttpContext.Session);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegistroViewModel model)
        {
            // ── Validar que el nombre de usuario no use prefijos reservados ──
            if (!string.IsNullOrWhiteSpace(model.NombreUsuario))
            {
                var nombreLower = model.NombreUsuario.ToLower();
                if (nombreLower.StartsWith("cliente_") || nombreLower.StartsWith("proveedor_"))
                    ModelState.AddModelError("NombreUsuario",
                        "Ese nombre de usuario está reservado por el sistema. Por favor elegí otro.");
            }

            if (model.TipoUsuario == "Cliente")
            {
                ModelState.Remove("Cuit");
                ModelState.Remove("RazonSocial");
                ModelState.Remove("Telefono");
                ModelState.Remove("DireccionProveedor");

                if (!model.ClienteId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(model.Documento))
                        ModelState.AddModelError("Documento", "El documento es obligatorio.");
                    if (string.IsNullOrWhiteSpace(model.Nombre))
                        ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
                    if (string.IsNullOrWhiteSpace(model.Apellido))
                        ModelState.AddModelError("Apellido", "El apellido es obligatorio.");
                    if (string.IsNullOrWhiteSpace(model.Direccion))
                        ModelState.AddModelError("Direccion", "La dirección es obligatoria.");
                }
            }
            else if (model.TipoUsuario == "Proveedor")
            {
                ModelState.Remove("Documento");
                ModelState.Remove("Nombre");
                ModelState.Remove("Apellido");
                ModelState.Remove("Direccion");

                if (!model.ProveedorId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(model.Cuit))
                        ModelState.AddModelError("Cuit", "El CUIT es obligatorio.");
                    if (string.IsNullOrWhiteSpace(model.RazonSocial))
                        ModelState.AddModelError("RazonSocial", "La razón social es obligatoria.");
                    if (string.IsNullOrWhiteSpace(model.Telefono))
                        ModelState.AddModelError("Telefono", "El teléfono es obligatorio.");
                    if (string.IsNullOrWhiteSpace(model.DireccionProveedor))
                        ModelState.AddModelError("DireccionProveedor", "La dirección es obligatoria.");
                }
            }

            if (!ModelState.IsValid) return View(model);

            try
            {
                var usuario = new Usuario
                {
                    NombreUsuario = model.NombreUsuario,
                    Contraseña = model.Contraseña,
                    Email = model.Email,
                    Estado = true,
                    FechaCreacion = DateTime.Now,
                    IntentosIngreso = 0,
                    Rol = model.TipoUsuario
                };

                if (model.TipoUsuario == "Cliente")
                {
                    if (model.ClienteId.HasValue)
                    {
                        _controladoraUsuario.VincularUsuarioAClienteExistente(usuario, model.ClienteId.Value);
                    }
                    else
                    {
                        _controladoraUsuario.RegistrarCliente(usuario, new Cliente
                        {
                            Documento = model.Documento,
                            Nombre = model.Nombre,
                            Apellido = model.Apellido,
                            Direccion = model.Direccion
                        });
                    }
                }
                else if (model.TipoUsuario == "Proveedor")
                {
                    if (model.ProveedorId.HasValue)
                    {
                        _controladoraUsuario.VincularUsuarioAProveedorExistente(usuario, model.ProveedorId.Value);
                    }
                    else
                    {
                        _controladoraUsuario.RegistrarProveedor(usuario, new Proveedor
                        {
                            Cuit = model.Cuit,
                            RazonSocial = model.RazonSocial,
                            Telefono = model.Telefono,
                            Email = model.Email,
                            Direccion = model.DireccionProveedor
                        });
                    }
                }

                TempData["Success"] = $"{model.TipoUsuario} registrado exitosamente.";

                if (SessionHelper.EsAdministrador(HttpContext.Session))
                {
                    TempData["ActivePanel"] = model.TipoUsuario == "Cliente" ? "clientes" : "proveedores";
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public JsonResult VerificarDni(string documento)
        {
            try
            {
                var cliente = _controladoraUsuario.ObtenerClientes()
                    .FirstOrDefault(c => c.Documento == documento);

                if (cliente == null)
                    return Json(new { existe = false });

                if (cliente.UsuarioId.HasValue)
                {
                    var usuario = _controladoraUsuario.ObtenerUsuarioPorId(cliente.UsuarioId.Value);
                    return Json(new
                    {
                        existe = true,
                        tieneUsuario = true,
                        nombreUsuario = usuario?.NombreUsuario ?? $"cliente_{documento}"
                    });
                }

                return Json(new
                {
                    existe = true,
                    tieneUsuario = false,
                    clienteId = cliente.Id,
                    nombre = cliente.Nombre,
                    apellido = cliente.Apellido,
                    direccion = cliente.Direccion
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult VerificarCuit(string cuit)
        {
            try
            {
                var proveedor = _controladoraProveedor.ObtenerProveedores()
                    .FirstOrDefault(p => p.Cuit == cuit);

                if (proveedor == null)
                    return Json(new { existe = false });

                if (proveedor.UsuarioId.HasValue)
                {
                    var usuario = _controladoraUsuario.ObtenerUsuarioPorId(proveedor.UsuarioId.Value);
                    return Json(new
                    {
                        existe = true,
                        tieneUsuario = true,
                        nombreUsuario = usuario?.NombreUsuario ?? $"proveedor_{cuit}"
                    });
                }

                return Json(new
                {
                    existe = true,
                    tieneUsuario = false,
                    proveedorId = proveedor.Id,
                    razonSocial = proveedor.RazonSocial,
                    telefono = proveedor.Telefono,
                    direccion = proveedor.Direccion
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult RecuperarClave() => View();

        [HttpPost]
        public IActionResult RecuperarClave(string nombreUsuario, string email)
        {
            try
            {
                var apiKey = _config["ResendApiKey"];
                var resultado = _controladoraUsuario.SolicitarRecuperacionClave(nombreUsuario, email, apiKey);
                TempData[resultado ? "Success" : "Error"] = resultado
                    ? "✓ Te enviamos una nueva contraseña a tu email."
                    : "✗ No encontramos un usuario con esos datos.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Login");
        }

        private string ObtenerIPReal()
        {
            var xForwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xForwardedFor))
            {
                var primeraIp = xForwardedFor
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(primeraIp)) return primeraIp;
            }

            var trueClientIp = HttpContext.Request.Headers["True-Client-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(trueClientIp)) return trueClientIp.Trim();

            var cfIp = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(cfIp)) return cfIp.Trim();

            var xRealIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xRealIp)) return xRealIp.Trim();

            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Desconocida";
            if (remoteIp == "::1") return "127.0.0.1";
            if (remoteIp.StartsWith("::ffff:")) return remoteIp[7..];
            return remoteIp;
        }

        private IActionResult RedirectToRol()
        {
            if (SessionHelper.EsAdministrador(HttpContext.Session)) return RedirectToAction("Index", "Admin");
            if (SessionHelper.EsProveedor(HttpContext.Session)) return RedirectToAction("Index", "Proveedor");
            if (SessionHelper.EsCliente(HttpContext.Session)) return RedirectToAction("Index", "PanelCliente");
            return RedirectToAction("Login");
        }
    }
}