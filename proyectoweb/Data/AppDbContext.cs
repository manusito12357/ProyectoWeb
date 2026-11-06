using Microsoft.EntityFrameworkCore;
using proyectoweb.Models;

namespace proyectoweb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Inscripcion> Inscripciones{ get; set; }
        public DbSet<Leccion> Lecciones { get; set; }
        public DbSet<LeccionCompletada> LeccionesCompletadas { get; set; }
    }
}
