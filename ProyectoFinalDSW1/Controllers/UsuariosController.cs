using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;
using ProyectoFinalDSW1.Services;

namespace ProyectoFinalDSW1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private const int PageSize = 8;

        private readonly AppDbContext _context;
        private readonly TallerRepository _repo;

        public UsuariosController(AppDbContext context, TallerRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        private int IdActual() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET: Usuarios
        public async Task<IActionResult> Index(string? buscar, int page = 1)
        {
            if (page < 1) page = 1;
            var (items, total) = await _repo.ListarUsuarios(buscar, page, PageSize);
            return View(ListaPaginada<Usuario>.Crear(items, total, page, PageSize, buscar));
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View(new UsuarioCrearViewModel());
        }

        // POST: Usuarios/Create
        [HttpPost]
        public async Task<IActionResult> Create(UsuarioCrearViewModel modelo)
        {
            var correo = modelo.Correo?.Trim().ToLowerInvariant() ?? string.Empty;

            if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
            {
                ModelState.AddModelError(nameof(modelo.Correo), "Ya existe un usuario con ese correo");
            }

            if (!ModelState.IsValid)
            {
                modelo.Clave = string.Empty;
                modelo.ConfirmarClave = string.Empty;
                return View(modelo);
            }

            var usuario = new Usuario
            {
                Nombre = modelo.Nombre.Trim(),
                Correo = correo,
                Rol = modelo.Rol!.Value,
                ClaveHash = ClaveSegura.Hashear(modelo.Clave),
                Activo = true,
                FechaRegistro = DateTime.Now
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Usuario {usuario.Nombre} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            return View(new UsuarioEditarViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Activo = usuario.Activo
            });
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UsuarioEditarViewModel modelo)
        {
            if (id != modelo.Id) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var correo = modelo.Correo?.Trim().ToLowerInvariant() ?? string.Empty;

            if (await _context.Usuarios.AnyAsync(u => u.Correo == correo && u.Id != id))
            {
                ModelState.AddModelError(nameof(modelo.Correo), "Ya existe un usuario con ese correo");
            }

            // El administrador no puede quitarse el acceso a sí mismo
            if (id == IdActual() && (!modelo.Activo || modelo.Rol != Rol.Admin))
            {
                ModelState.AddModelError(string.Empty, "No puedes desactivar tu cuenta ni cambiar tu propio rol.");
            }

            if (!ModelState.IsValid)
            {
                modelo.NuevaClave = null;
                return View(modelo);
            }

            usuario.Nombre = modelo.Nombre.Trim();
            usuario.Correo = correo;
            usuario.Rol = modelo.Rol!.Value;
            usuario.Activo = modelo.Activo;

            if (!string.IsNullOrWhiteSpace(modelo.NuevaClave))
            {
                usuario.ClaveHash = ClaveSegura.Hashear(modelo.NuevaClave);
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = null;
            }

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Usuarios/Desbloquear/5
        [HttpPost]
        public async Task<IActionResult> Desbloquear(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Cuenta de {usuario.Nombre} desbloqueada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
