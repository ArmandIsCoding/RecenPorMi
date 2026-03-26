using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecenPorMi.Data.Models;

namespace RecenPorMi.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Peticion> Peticiones { get; set; }
        public DbSet<Rezo> Rezos { get; set; }
        public DbSet<PeticionImagen> PeticionImagenes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relación Peticion -> Usuario
            modelBuilder.Entity<Peticion>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de relaciones Rezo -> Peticion
            modelBuilder.Entity<Rezo>()
                .HasOne(r => r.Peticion)
                .WithMany(p => p.Rezos)
                .HasForeignKey(r => r.PeticionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Configuración de relación PeticionImagen -> Peticion
            modelBuilder.Entity<PeticionImagen>()
                .HasOne(pi => pi.Peticion)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(pi => pi.PeticionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para mejorar el rendimiento
            modelBuilder.Entity<Peticion>()
                .HasIndex(p => p.FechaPublicacion);

            modelBuilder.Entity<Peticion>()
                .HasIndex(p => p.UserId);

            modelBuilder.Entity<Rezo>()
                .HasIndex(r => new { r.PeticionId, r.IpHash, r.Fecha });

            modelBuilder.Entity<PeticionImagen>()
                .HasIndex(pi => pi.PeticionId);
        }
    }
}
