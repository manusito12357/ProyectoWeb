using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoweb.Data;
using proyectoweb.Models;

namespace proyectoweb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PanelController : Controller
    {
        private readonly AppDbContext _context;

        public PanelController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var suscriptores = _context.UserRoles
                .Join(_context.Roles.Where(r => r.Name == "Suscriptor"), ur => ur.RoleId, r => r.Id, (ur, r) => ur);

            var resumen = new ResumenPanel
            {
                Cursos = await _context.Cursos.CountAsync(),
                Publicados = await _context.Cursos.CountAsync(c => c.Publicado),
                Categorias = await _context.Categorias.CountAsync(),
                Lecciones = await _context.Lecciones.CountAsync(),
                Suscriptores = await suscriptores.CountAsync(),
                Inscripciones = await _context.Inscripciones.CountAsync(),
                Terminados = await _context.Inscripciones.CountAsync(i => i.FechaFinalizacion != null),
                LeccionesCompletadas = await _context.LeccionesCompletadas.CountAsync(),
                Populares = await _context.Cursos
                    .Select(c => new CursoInscritos
                    {
                        Id = c.Id,
                        Titulo = c.Titulo,
                        Inscritos = _context.Inscripciones.Count(i => i.CursoId == c.Id)
                    })
                    .OrderByDescending(c => c.Inscritos)
                    .ThenBy(c => c.Titulo)
                    .Take(5)
                    .ToListAsync()
            };

            return View(resumen);
        }
    }
}
