using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehiculosController : Controller
    {
        private const int PageSize = 8;

        private readonly AppDbContext _context;
        private readonly TallerRepository _repo;

        public VehiculosController(AppDbContext context, TallerRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        private async Task CargarClientes(int? clienteActual = null)
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Activo || c.Id == clienteActual)
                .OrderBy(c => c.Nombre)
                .Select(c => new { c.Id, Texto = c.Nombre + " - DNI " + c.Dni })
                .ToListAsync();

            ViewBag.Clientes = new SelectList(clientes, "Id", "Texto", clienteActual);
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index(string? buscar, int page = 1)
        {
            if (page < 1) page = 1;
            var (items, total) = await _repo.ListarVehiculos(buscar, page, PageSize);
            return View(ListaPaginada<Vehiculo>.Crear(items, total, page, PageSize, buscar));
        }

        // GET: Vehiculos/Create
        public async Task<IActionResult> Create()
        {
            await CargarClientes();
            return View("Form", new Vehiculo { Anio = DateTime.Now.Year });
        }

        // POST: Vehiculos/Create
        // Al registrar un vehículo se crea automáticamente su orden de atención (Pendiente)
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Placa,Marca,Modelo,Anio,Kilometraje,ClienteId")] Vehiculo vehiculo)
        {
            vehiculo.Placa = (vehiculo.Placa ?? string.Empty).Trim().ToUpperInvariant();

            if (await _context.Vehiculos.AnyAsync(v => v.Placa == vehiculo.Placa))
            {
                ModelState.AddModelError(nameof(vehiculo.Placa), "Ya existe un vehículo con esa placa");
            }

            if (!await _context.Clientes.AnyAsync(c => c.Id == vehiculo.ClienteId && c.Activo))
            {
                ModelState.AddModelError(nameof(vehiculo.ClienteId), "Seleccione un cliente válido");
            }

            if (!ModelState.IsValid)
            {
                await CargarClientes(vehiculo.ClienteId);
                return View("Form", vehiculo);
            }

            var orden = new OrdenTrabajo
            {
                Vehiculo = vehiculo,
                Estado = EstadoOrden.Pendiente,
                FechaIngreso = DateTime.Now
            };

            _context.Vehiculos.Add(vehiculo);
            _context.OrdenesTrabajo.Add(orden);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Vehículo {vehiculo.Placa} registrado. Se creó la orden N° {orden.Id} en estado Pendiente.";
            return RedirectToAction("Details", "Ordenes", new { id = orden.Id });
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehiculo = await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return NotFound();

            await CargarClientes(vehiculo.ClienteId);
            return View("Form", vehiculo);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Placa,Marca,Modelo,Anio,Kilometraje,ClienteId")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id) return NotFound();

            vehiculo.Placa = (vehiculo.Placa ?? string.Empty).Trim().ToUpperInvariant();

            if (await _context.Vehiculos.AnyAsync(v => v.Placa == vehiculo.Placa && v.Id != id))
            {
                ModelState.AddModelError(nameof(vehiculo.Placa), "Ya existe un vehículo con esa placa");
            }

            if (!await _context.Clientes.AnyAsync(c => c.Id == vehiculo.ClienteId))
            {
                ModelState.AddModelError(nameof(vehiculo.ClienteId), "Seleccione un cliente válido");
            }

            if (!ModelState.IsValid)
            {
                await CargarClientes(vehiculo.ClienteId);
                return View("Form", vehiculo);
            }

            var existente = await _context.Vehiculos.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Placa = vehiculo.Placa;
            existente.Marca = vehiculo.Marca.Trim();
            existente.Modelo = vehiculo.Modelo.Trim();
            existente.Anio = vehiculo.Anio;
            existente.Kilometraje = vehiculo.Kilometraje;
            existente.ClienteId = vehiculo.ClienteId;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Vehículo actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Vehiculos/NuevaOrden/5  (el vehículo vuelve al taller)
        [HttpPost]
        public async Task<IActionResult> NuevaOrden(int id)
        {
            if (!await _context.Vehiculos.AnyAsync(v => v.Id == id))
            {
                return NotFound();
            }

            var abierta = await _context.OrdenesTrabajo
                .AnyAsync(o => o.VehiculoId == id && o.Estado != EstadoOrden.Entregado);
            if (abierta)
            {
                TempData["Error"] = "Este vehículo ya tiene una orden abierta.";
                return RedirectToAction(nameof(Index));
            }

            var orden = new OrdenTrabajo
            {
                VehiculoId = id,
                Estado = EstadoOrden.Pendiente,
                FechaIngreso = DateTime.Now
            };
            _context.OrdenesTrabajo.Add(orden);
            await _context.SaveChangesAsync();

            TempData["Ok"] = $"Se creó la orden N° {orden.Id} en estado Pendiente.";
            return RedirectToAction("Details", "Ordenes", new { id = orden.Id });
        }

        // POST: Vehiculos/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return NotFound();

            if (await _context.OrdenesTrabajo.AnyAsync(o => o.VehiculoId == id))
            {
                TempData["Error"] = "No se puede eliminar: el vehículo tiene órdenes registradas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Vehículo eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
