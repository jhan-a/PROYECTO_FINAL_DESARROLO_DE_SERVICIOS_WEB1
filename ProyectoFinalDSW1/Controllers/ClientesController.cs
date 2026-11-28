using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientesController : Controller
    {
        private const int PageSize = 8;

        private readonly AppDbContext _context;
        private readonly TallerRepository _repo;

        public ClientesController(AppDbContext context, TallerRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(string? buscar, int page = 1)
        {
            if (page < 1) page = 1;
            var (items, total) = await _repo.ListarClientes(buscar, page, PageSize);
            return View(ListaPaginada<Cliente>.Crear(items, total, page, PageSize, buscar));
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View("Form", new Cliente());
        }

        // POST: Clientes/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Nombre,Dni,Telefono,Correo,Activo")] Cliente cliente)
        {
            if (await _context.Clientes.AnyAsync(c => c.Dni == cliente.Dni))
            {
                ModelState.AddModelError(nameof(cliente.Dni), "Ya existe un cliente con ese DNI");
            }

            if (!ModelState.IsValid)
            {
                return View("Form", cliente);
            }

            cliente.Nombre = cliente.Nombre.Trim();
            cliente.Correo = cliente.Correo.Trim().ToLowerInvariant();
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Cliente {cliente.Nombre} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return NotFound();

            return View("Form", cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Dni,Telefono,Correo,Activo")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (await _context.Clientes.AnyAsync(c => c.Dni == cliente.Dni && c.Id != id))
            {
                ModelState.AddModelError(nameof(cliente.Dni), "Ya existe un cliente con ese DNI");
            }

            if (!ModelState.IsValid)
            {
                return View("Form", cliente);
            }

            var existente = await _context.Clientes.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre = cliente.Nombre.Trim();
            existente.Dni = cliente.Dni;
            existente.Telefono = cliente.Telefono;
            existente.Correo = cliente.Correo.Trim().ToLowerInvariant();
            existente.Activo = cliente.Activo;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Cliente actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
