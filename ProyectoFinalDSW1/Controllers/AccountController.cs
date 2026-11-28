using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;
using ProyectoFinalDSW1.Services;

namespace ProyectoFinalDSW1.Controllers
{
    public class AccountController : Controller
    {
        private const int MaximoIntentos = 5;
        private const int MinutosBloqueo = 15;

        // Hash falso para que el tiempo de respuesta sea igual cuando el correo no existe
        private static readonly string HashFalso = ClaveSegura.Hashear("clave-falsa-para-igualar-tiempos");

        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.Clave = string.Empty;
                return View(modelo);
            }

            var correo = modelo.Correo.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                ClaveSegura.Verificar(modelo.Clave, HashFalso);
                return ErrorLogin(modelo, "Correo o contraseña incorrectos");
            }

            if (usuario.BloqueadoHasta != null && usuario.BloqueadoHasta > DateTime.Now)
            {
                return ErrorLogin(modelo, "Cuenta bloqueada temporalmente por intentos fallidos. Intente más tarde.");
            }

            var claveCorrecta = ClaveSegura.Verificar(modelo.Clave, usuario.ClaveHash);

            if (!claveCorrecta || !usuario.Activo)
            {
                if (!claveCorrecta)
                {
                    usuario.IntentosFallidos++;
                    if (usuario.IntentosFallidos >= MaximoIntentos)
                    {
                        usuario.BloqueadoHasta = DateTime.Now.AddMinutes(MinutosBloqueo);
                        usuario.IntentosFallidos = 0;
                        _logger.LogWarning("Cuenta bloqueada por intentos fallidos. UsuarioId: {Id}", usuario.Id);
                    }
                    await _context.SaveChangesAsync();
                }
                return ErrorLogin(modelo, "Correo o contraseña incorrectos");
            }

            // Inicio de sesión correcto
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                new AuthenticationProperties { IsPersistent = false });

            // Solo se redirige a direcciones del mismo sitio (evita Open Redirect)
            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
            {
                return Redirect(modelo.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/CambiarClave
        public IActionResult CambiarClave()
        {
            return View(new CambiarClaveViewModel());
        }

        // POST: Account/CambiarClave
        [HttpPost]
        public async Task<IActionResult> CambiarClave(CambiarClaveViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ClaveSegura.Verificar(modelo.ClaveActual, usuario.ClaveHash))
            {
                ModelState.AddModelError(nameof(modelo.ClaveActual), "La contraseña actual es incorrecta");
                return View(modelo);
            }

            usuario.ClaveHash = ClaveSegura.Hashear(modelo.ClaveNueva);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccesoDenegado
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        private IActionResult ErrorLogin(LoginViewModel modelo, string mensaje)
        {
            modelo.Clave = string.Empty;
            ModelState.AddModelError(string.Empty, mensaje);
            return View(modelo);
        }
    }
}
