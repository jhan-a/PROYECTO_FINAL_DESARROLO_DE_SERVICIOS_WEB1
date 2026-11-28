using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RepuestosController : Controller
    {
        private const int PageSize = 8;

        private readonly AppDbContext _context;
        private readonly TallerRepository _repo;

        public RepuestosController(AppDbContext context, TallerRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        // GET: Repuestos  (alerta = true muestra solo los repuestos en stock mínimo)
        public async Task<IActionResult> Index(string? buscar, bool alerta = false, int page = 1)
        {
            if (page < 1) page = 1;
            var (items, total) = await _repo.ListarRepuestos(buscar, alerta, page, PageSize);
            ViewBag.SoloAlerta = alerta;
            var lista = ListaPaginada<Repuesto>.Crear(items, total, page, PageSize, buscar);
            lista.Alerta = alerta;
            return View(lista);
        }

        // GET: Repuestos/Create
        public IActionResult Create()
        {
            return View("Form", new Repuesto());
        }

        // POST: Repuestos/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Nombre,Precio,Stock,StockMinimo,Activo")] Repuesto repuesto)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", repuesto);
            }

            repuesto.Nombre = repuesto.Nombre.Trim();
            _context.Repuestos.Add(repuesto);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Repuesto {repuesto.Nombre} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Repuestos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var repuesto = await _context.Repuestos.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (repuesto == null) return NotFound();

            return View("Form", repuesto);
        }

        // POST: Repuestos/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Precio,Stock,StockMinimo,Activo")] Repuesto repuesto)
        {
            if (id != repuesto.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View("Form", repuesto);
            }

            var existente = await _context.Repuestos.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre = repuesto.Nombre.Trim();
            existente.Precio = repuesto.Precio;
            existente.Stock = repuesto.Stock;
            existente.StockMinimo = repuesto.StockMinimo;
            existente.Activo = repuesto.Activo;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Repuesto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
