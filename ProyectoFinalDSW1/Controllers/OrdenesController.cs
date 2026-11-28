using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Controllers
{
    // Admin y Mecánico entran aquí. El mecánico solo ve y trabaja sus propias órdenes.
    public class OrdenesController : Controller
    {
        private const int PageSize = 8;

        private readonly AppDbContext _context;
        private readonly TallerRepository _repo;

        public OrdenesController(AppDbContext context, TallerRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        private int IdActual() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private bool PuedeOperar(OrdenTrabajo orden) => User.IsInRole("Admin") || orden.MecanicoId == IdActual();

        // GET: Ordenes
        public async Task<IActionResult> Index(string? buscar, int? estado, int page = 1)
        {
            if (page < 1) page = 1;
            if (estado != null && !Enum.IsDefined(typeof(EstadoOrden), estado.Value)) estado = null;

            int? mecanicoId = User.IsInRole("Mecanico") ? IdActual() : null;
            var (items, total) = await _repo.ListarOrdenes(buscar, estado, mecanicoId, page, PageSize);

            return View(ListaPaginada<OrdenTrabajo>.Crear(items, total, page, PageSize, buscar, estado));
        }

        // GET: Ordenes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var orden = await _context.OrdenesTrabajo
                .AsNoTracking()
                .Include(o => o.Vehiculo).ThenInclude(v => v!.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Detalles).ThenInclude(d => d.Servicio)
                .Include(o => o.Detalles).ThenInclude(d => d.Repuesto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null) return NotFound();
            if (!PuedeOperar(orden)) return Forbid();

            if (User.IsInRole("Admin"))
            {
                var mecanicos = await _context.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Rol == Rol.Mecanico && u.Activo)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();
                ViewBag.Mecanicos = new SelectList(mecanicos, "Id", "Nombre", orden.MecanicoId);
            }

            if (orden.Estado == EstadoOrden.EnReparacion)
            {
                var servicios = (await _context.Servicios
                        .AsNoTracking()
                        .Where(s => s.Activo)
                        .OrderBy(s => s.Nombre)
                        .ToListAsync())
                    .Select(s => new { s.Id, Texto = $"{s.Nombre} - S/ {s.Precio:N2}" })
                    .ToList();
                ViewBag.Servicios = new SelectList(servicios, "Id", "Texto");

                var repuestos = (await _context.Repuestos
                        .AsNoTracking()
                        .Where(r => r.Activo && r.Stock > 0)
                        .OrderBy(r => r.Nombre)
                        .ToListAsync())
                    .Select(r => new { r.Id, Texto = $"{r.Nombre} - S/ {r.Precio:N2} (stock {r.Stock})" })
                    .ToList();
                ViewBag.Repuestos = new SelectList(repuestos, "Id", "Texto");
            }

            return View(orden);
        }

        // POST: Ordenes/Asignar/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Asignar(int id, int? mecanicoId)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return NotFound();

            if (orden.Estado == EstadoOrden.Entregado)
            {
                TempData["Error"] = "La orden ya fue entregada y no se puede modificar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (mecanicoId == null || !await _context.Usuarios.AnyAsync(u => u.Id == mecanicoId && u.Rol == Rol.Mecanico && u.Activo))
            {
                TempData["Error"] = "Seleccione un mecánico válido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            orden.MecanicoId = mecanicoId;
            if (!await GuardarOrden())
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Ok"] = "Mecánico asignado correctamente.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Ordenes/CambiarEstado/5
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, EstadoOrden nuevoEstado)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return NotFound();
            if (!PuedeOperar(orden)) return Forbid();

            if (!Enum.IsDefined(typeof(EstadoOrden), nuevoEstado))
            {
                return BadRequest();
            }

            // El estado solo avanza en orden: Pendiente > En reparación > Terminado > Entregado
            var valido =
                (orden.Estado == EstadoOrden.Pendiente && nuevoEstado == EstadoOrden.EnReparacion) ||
                (orden.Estado == EstadoOrden.EnReparacion && nuevoEstado == EstadoOrden.Terminado) ||
                (orden.Estado == EstadoOrden.Terminado && nuevoEstado == EstadoOrden.Entregado);

            if (!valido)
            {
                TempData["Error"] = "Cambio de estado no permitido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (nuevoEstado == EstadoOrden.EnReparacion && orden.MecanicoId == null)
            {
                TempData["Error"] = "Primero debe asignar un mecánico a la orden.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Solo el administrador entrega el vehículo
            if (nuevoEstado == EstadoOrden.Entregado)
            {
                if (!User.IsInRole("Admin")) return Forbid();
                orden.FechaEntrega = DateTime.Now;
            }

            orden.Estado = nuevoEstado;
            if (!await GuardarOrden())
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Ok"] = $"La orden ahora está en estado: {nuevoEstado.Texto()}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Ordenes/AgregarServicio/5
        [HttpPost]
        public async Task<IActionResult> AgregarServicio(int id, int servicioId)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return NotFound();
            if (!PuedeOperar(orden)) return Forbid();

            if (orden.Estado != EstadoOrden.EnReparacion)
            {
                TempData["Error"] = "Solo se pueden agregar servicios mientras la orden está En reparación.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var servicio = await _context.Servicios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == servicioId && s.Activo);
            if (servicio == null)
            {
                TempData["Error"] = "Seleccione un servicio válido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.DetallesOrden.Add(new DetalleOrden
            {
                OrdenTrabajoId = id,
                ServicioId = servicio.Id,
                Cantidad = 1,
                Subtotal = servicio.Precio
            });
            orden.CostoTotal += servicio.Precio;

            if (!await GuardarOrden())
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Ok"] = $"Servicio agregado: {servicio.Nombre}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Ordenes/AgregarRepuesto/5
        // Descuenta el stock, calcula el subtotal, actualiza el costo y avisa si llega al stock mínimo
        [HttpPost]
        public async Task<IActionResult> AgregarRepuesto(int id, int repuestoId, int cantidad)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return NotFound();
            if (!PuedeOperar(orden)) return Forbid();

            if (orden.Estado != EstadoOrden.EnReparacion)
            {
                TempData["Error"] = "Solo se pueden agregar repuestos mientras la orden está En reparación.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (cantidad < 1 || cantidad > 1000)
            {
                TempData["Error"] = "La cantidad debe ser entre 1 y 1000.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            // Descuento atómico: solo descuenta si hay stock suficiente
            var filas = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Repuestos SET Stock = Stock - {cantidad} WHERE Id = {repuestoId} AND Activo = 1 AND Stock >= {cantidad}");

            if (filas == 0)
            {
                TempData["Error"] = "Repuesto no disponible o sin stock suficiente.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var repuesto = await _context.Repuestos.AsNoTracking().FirstAsync(r => r.Id == repuestoId);
            var subtotal = repuesto.Precio * cantidad;

            _context.DetallesOrden.Add(new DetalleOrden
            {
                OrdenTrabajoId = id,
                RepuestoId = repuestoId,
                Cantidad = cantidad,
                Subtotal = subtotal
            });
            orden.CostoTotal += subtotal;

            try
            {
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "La orden fue modificada por otra persona. Intente de nuevo.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Ok"] = $"Repuesto agregado: {repuesto.Nombre} x {cantidad}.";
            if (repuesto.Stock <= repuesto.StockMinimo)
            {
                TempData["Alerta"] = $"Alerta: el repuesto {repuesto.Nombre} llegó al stock mínimo (quedan {repuesto.Stock}).";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> GuardarOrden()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "La orden fue modificada por otra persona. Intente de nuevo.";
                return false;
            }
        }
    }
}
