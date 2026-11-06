using Microsoft.EntityFrameworkCore;
using proyectoweb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace proyectoweb.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Inscripcion> Inscripciones{ get; set; }
        public DbSet<Leccion> Lecciones { get; set; }
        public DbSet<LeccionCompletada> LeccionesCompletadas { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Curso>()
                .HasOne(c => c.Categoria)
                .WithMany(cat => cat.Cursos)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
