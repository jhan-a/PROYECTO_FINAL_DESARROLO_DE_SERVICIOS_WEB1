using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Models;

namespace ProyectoFinalDSW1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
        public DbSet<DetalleOrden> DetallesOrden { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nombres de tablas fijos (los usan los procedimientos almacenados)
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Cliente>().ToTable("Clientes");
            modelBuilder.Entity<Vehiculo>().ToTable("Vehiculos");
            modelBuilder.Entity<Servicio>().ToTable("Servicios");
            modelBuilder.Entity<Repuesto>().ToTable("Repuestos");
            modelBuilder.Entity<OrdenTrabajo>().ToTable("OrdenesTrabajo");
            modelBuilder.Entity<DetalleOrden>().ToTable("DetallesOrden");

            // Datos únicos
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Correo).IsUnique();
            modelBuilder.Entity<Cliente>().HasIndex(c => c.Dni).IsUnique();
            modelBuilder.Entity<Vehiculo>().HasIndex(v => v.Placa).IsUnique();

            // Relaciones (sin borrado en cascada, para no perder historial)
            modelBuilder.Entity<Vehiculo>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Vehiculos)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTrabajo>()
                .HasOne(o => o.Vehiculo)
                .WithMany(v => v.Ordenes)
                .HasForeignKey(o => o.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTrabajo>()
                .HasOne(o => o.Mecanico)
                .WithMany()
                .HasForeignKey(o => o.MecanicoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleOrden>()
                .HasOne(d => d.OrdenTrabajo)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleOrden>()
                .HasOne(d => d.Servicio)
                .WithMany()
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleOrden>()
                .HasOne(d => d.Repuesto)
                .WithMany()
                .HasForeignKey(d => d.RepuestoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
