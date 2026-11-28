using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiciosController : Controller
    {
        private const int PageSize = 8;

        private readonly AppDbContext _context;
        private readonly TallerRepository _repo;

        public ServiciosController(AppDbContext context, TallerRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        // GET: Servicios
        public async Task<IActionResult> Index(string? buscar, int page = 1)
        {
            if (page < 1) page = 1;
            var (items, total) = await _repo.ListarServicios(buscar, page, PageSize);
            return View(ListaPaginada<Servicio>.Crear(items, total, page, PageSize, buscar));
        }

        // GET: Servicios/Create
        public IActionResult Create()
        {
            return View("Form", new Servicio());
        }

        // POST: Servicios/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,Activo")] Servicio servicio)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", servicio);
            }

            servicio.Nombre = servicio.Nombre.Trim();
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Servicio {servicio.Nombre} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Servicios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var servicio = await _context.Servicios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (servicio == null) return NotFound();

            return View("Form", servicio);
        }

        // POST: Servicios/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Activo")] Servicio servicio)
        {
            if (id != servicio.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View("Form", servicio);
            }

            var existente = await _context.Servicios.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre = servicio.Nombre.Trim();
            existente.Descripcion = servicio.Descripcion;
            existente.Precio = servicio.Precio;
            existente.Activo = servicio.Activo;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Servicio actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
