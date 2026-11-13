using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var publicados = _context.Cursos.Where(c => c.Publicado);
            if (!string.IsNullOrEmpty(buscar))
            {
                publicados = publicados.Where(c => EF.Functions.Collate(c.Titulo, "Latin1_General_CI_AI").Contains(buscar));
            }

            var porCategoria = await publicados
                .GroupBy(c => c.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.CategoriaId, g => g.Cantidad);
            var categorias = (await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync())
                .Select(c => new CategoriaConteo { Id = c.Id, Nombre = c.Nombre, Cantidad = porCategoria.GetValueOrDefault(c.Id) })
                .Where(c => c.Cantidad > 0 || c.Id == categoriaId)
                .ToList();

            IQueryable<Curso> cursos = publicados.Include(c => c.Categoria);
            if (categoriaId != null)
            {
                cursos = cursos.Where(c => c.CategoriaId == categoriaId);
            }

            var total = await cursos.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)CursosPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var lista = await cursos
                .OrderByDescending(c => c.FechaCreacion)
                .ThenBy(c => c.Id)
                .Skip((pagina - 1) * CursosPorPagina)
                .Take(CursosPorPagina)
                .ToListAsync();

            var ids = lista.Select(c => c.Id).ToList();
            var lecciones = await _context.Lecciones
                .Where(l => ids.Contains(l.CursoId))
                .GroupBy(l => l.CursoId)
                .Select(g => new { CursoId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.CursoId, g => g.Cantidad);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inscripciones = new Dictionary<int, bool>();
            var completadas = new Dictionary<int, int>();
            if (userId != null)
            {
                inscripciones = await _context.Inscripciones
                    .Where(i => i.UserId == userId && ids.Contains(i.CursoId))
                    .ToDictionaryAsync(i => i.CursoId, i => i.FechaFinalizacion != null);
                completadas = await _context.LeccionesCompletadas
                    .Where(lc => lc.UserId == userId && ids.Contains(lc.Leccion!.CursoId))
                    .GroupBy(lc => lc.Leccion!.CursoId)
                    .Select(g => new { CursoId = g.Key, Cantidad = g.Count() })
                    .ToDictionaryAsync(g => g.CursoId, g => g.Cantidad);
            }

            return View(new CatalogoViewModel
            {
                Cursos = lista.Select(c => new CursoCatalogo
                {
                    Curso = c,
                    Lecciones = lecciones.GetValueOrDefault(c.Id),
                    Completadas = completadas.GetValueOrDefault(c.Id),
                    Inscrito = inscripciones.ContainsKey(c.Id),
                    Terminado = inscripciones.GetValueOrDefault(c.Id)
                }).ToList(),
                Categorias = categorias,
                CategoriaId = categoriaId,
                Buscar = buscar,
                Total = total,
                Pagina = pagina,
                TotalPaginas = totalPaginas
            });
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
