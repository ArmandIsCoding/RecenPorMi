using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecenPorMi.Data.Models;

namespace RecenPorMi.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Peticion> Peticiones { get; set; }
        public DbSet<Rezo> Rezos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones
            modelBuilder.Entity<Rezo>()
                .HasOne(r => r.Peticion)
                .WithMany(p => p.Rezos)
                .HasForeignKey(r => r.PeticionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para mejorar el rendimiento
            modelBuilder.Entity<Peticion>()
                .HasIndex(p => p.FechaPublicacion);

            modelBuilder.Entity<Rezo>()
                .HasIndex(r => new { r.PeticionId, r.IpHash, r.Fecha });
        }
    }
}
