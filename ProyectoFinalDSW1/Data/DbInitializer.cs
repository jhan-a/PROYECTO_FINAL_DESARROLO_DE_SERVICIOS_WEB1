using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Models;
using ProyectoFinalDSW1.Services;

namespace ProyectoFinalDSW1.Data
{
    public static class DbInitializer
    {
        public static async Task InicializarAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Crea o actualiza las tablas con las migraciones
            await db.Database.MigrateAsync();

            // 2. Crea o actualiza los procedimientos almacenados (carpeta Data/Sp)
            var carpeta = Path.Combine(AppContext.BaseDirectory, "Data", "Sp");
            if (Directory.Exists(carpeta))
            {
                foreach (var archivo in Directory.GetFiles(carpeta, "*.sql").OrderBy(f => f))
                {
                    var sql = await File.ReadAllTextAsync(archivo);
                    await db.Database.ExecuteSqlRawAsync(sql);
                }
            }

            // 3. Usuarios iniciales (contraseña guardada con hash). Cámbialas al entrar.
            if (!await db.Usuarios.AnyAsync(u => u.Correo == "admin@taller.com"))
            {
                db.Usuarios.Add(new Usuario
                {
                    Nombre = "Administrador",
                    Correo = "admin@taller.com",
                    ClaveHash = ClaveSegura.Hashear("Admin#2026"),
                    Rol = Rol.Admin
                });
            }

            if (!await db.Usuarios.AnyAsync(u => u.Correo == "mecanico@taller.com"))
            {
                db.Usuarios.Add(new Usuario
                {
                    Nombre = "Mecánico Demo",
                    Correo = "mecanico@taller.com",
                    ClaveHash = ClaveSegura.Hashear("Mecanico#2026"),
                    Rol = Rol.Mecanico
                });
            }

            // 4. Catálogo inicial para poder probar
            if (!await db.Servicios.AnyAsync())
            {
                db.Servicios.AddRange(
                    new Servicio { Nombre = "Cambio de aceite", Descripcion = "Cambio de aceite y filtro", Precio = 80m },
                    new Servicio { Nombre = "Alineamiento y balanceo", Descripcion = "Alineamiento de las 4 ruedas", Precio = 120m },
                    new Servicio { Nombre = "Revisión de frenos", Descripcion = "Revisión y ajuste del sistema de frenos", Precio = 90m },
                    new Servicio { Nombre = "Diagnóstico computarizado", Descripcion = "Lectura de códigos de falla", Precio = 150m });
            }

            if (!await db.Repuestos.AnyAsync())
            {
                db.Repuestos.AddRange(
                    new Repuesto { Nombre = "Filtro de aceite", Precio = 25m, Stock = 20, StockMinimo = 5 },
                    new Repuesto { Nombre = "Pastillas de freno", Precio = 110m, Stock = 12, StockMinimo = 4 },
                    new Repuesto { Nombre = "Batería 12V", Precio = 380m, Stock = 6, StockMinimo = 2 },
                    new Repuesto { Nombre = "Bujía", Precio = 18m, Stock = 40, StockMinimo = 10 });
            }

            await db.SaveChangesAsync();
        }
    }
}
