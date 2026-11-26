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
    }
}
