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
        private const int CursosPorPagina = 9;

        private readonly AppDbContext _context;

        public CatalogoController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoriaId, string? buscar, int pagina = 1)
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

            var total = await cursos.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)CursosPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            ViewData["Pagina"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas;
            ViewData["CategoriaId"] = categoriaId;

            var lista = await cursos
                .OrderByDescending(c => c.FechaCreacion)
                .ThenBy(c => c.Id)
                .Skip((pagina - 1) * CursosPorPagina)
                .Take(CursosPorPagina)
                .ToListAsync();

            var ids = lista.Select(c => c.Id).ToList();
            ViewData["Lecciones"] = await _context.Lecciones
                .Where(l => ids.Contains(l.CursoId))
                .GroupBy(l => l.CursoId)
                .Select(g => new { CursoId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.CursoId, g => g.Cantidad);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["MisInscripciones"] = userId == null
                ? new Dictionary<int, bool>()
                : await _context.Inscripciones
                    .Where(i => i.UserId == userId && ids.Contains(i.CursoId))
                    .ToDictionaryAsync(i => i.CursoId, i => i.FechaFinalizacion != null);

            return View(lista);
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
            var inscripcion = userId == null
                ? null
                : await _context.Inscripciones.FirstOrDefaultAsync(i => i.UserId == userId && i.CursoId == curso.Id);
            ViewData["Inscrito"] = inscripcion != null;
            ViewData["Terminado"] = inscripcion?.FechaFinalizacion != null;

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
