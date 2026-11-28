using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ordenes = _context.OrdenesTrabajo.AsNoTracking();

            // El mecánico solo ve el resumen de sus órdenes
            if (User.IsInRole("Mecanico"))
            {
                var idMecanico = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                ordenes = ordenes.Where(o => o.MecanicoId == idMecanico);
            }

            var modelo = new DashboardViewModel
            {
                Pendientes = await ordenes.CountAsync(o => o.Estado == EstadoOrden.Pendiente),
                EnReparacion = await ordenes.CountAsync(o => o.Estado == EstadoOrden.EnReparacion),
                Terminadas = await ordenes.CountAsync(o => o.Estado == EstadoOrden.Terminado)
            };

            if (User.IsInRole("Admin"))
            {
                modelo.TotalClientes = await _context.Clientes.CountAsync(c => c.Activo);
                modelo.TotalVehiculos = await _context.Vehiculos.CountAsync();
                modelo.RepuestosEnAlerta = await _context.Repuestos
                    .AsNoTracking()
                    .Where(r => r.Activo && r.Stock <= r.StockMinimo)
                    .OrderBy(r => r.Stock)
                    .Take(5)
                    .ToListAsync();
            }

            return View(modelo);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
