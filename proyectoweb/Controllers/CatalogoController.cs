using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proyectoweb.Data;
using proyectoweb.Models;

namespace proyectoweb.Controllers
{
    [AllowAnonymous]
    public class CatalogoController : Controller
    {
        private readonly AppDbContext _context;

        public CatalogoController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoriaId, string? buscar)
        {
            buscar = buscar?.Trim();

            var cursos = _context.Cursos.Include(c => c.Categoria).Where(c => c.Publicado);
            if (categoriaId != null)
            {
                cursos = cursos.Where(c => c.CategoriaId == categoriaId);
            }
            if (!string.IsNullOrEmpty(buscar))
            {
                cursos = cursos.Where(c => EF.Functions.Collate(c.Titulo, "Latin1_General_CI_AI").Contains(buscar));
            }

            var categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            ViewData["Categorias"] = new SelectList(categorias, "Id", "Nombre", categoriaId);
            ViewData["Buscar"] = buscar;
            ViewData["Filtrado"] = categoriaId != null || !string.IsNullOrEmpty(buscar);

            return View(await cursos.OrderByDescending(c => c.FechaCreacion).ToListAsync());
        }

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .Include(c => c.Categoria)
                .Include(c => c.Lecciones!.OrderBy(l => l.Orden))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null || (!curso.Publicado && !User.IsInRole("Administrador")))
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["Inscrito"] = userId != null && await _context.EstaInscrito(userId, curso.Id);

            return View(curso);
        }

        [Authorize(Roles = "Suscriptor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Publicado);
            if (curso == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (!await _context.EstaInscrito(userId, id))
            {
                _context.Inscripciones.Add(new Inscripcion { UserId = userId, CursoId = id });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Curso", "MisCursos", new { id });
        }
    }
}
