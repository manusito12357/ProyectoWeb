using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoweb.Data;
using proyectoweb.Models;

namespace proyectoweb.Controllers
{
    public class CatalogoController : Controller
    {
        private const int CursosPorPagina = 9;
        private const int DuracionCorta = 120;
        private const int DuracionMedia = 300;

        private readonly AppDbContext _context;

        public CatalogoController(AppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(FiltroCatalogo filtro, int pagina = 1)
        {
            filtro.Normalizar();

            var publicados = _context.Cursos.Where(c => c.Publicado);
            if (!string.IsNullOrEmpty(filtro.Buscar))
            {
                publicados = publicados.Where(c => EF.Functions.Collate(c.Titulo, "Latin1_General_CI_AI").Contains(filtro.Buscar));
            }

            // Cada grupo de filtros cuenta sus opciones aplicando los demás filtros, pero no el propio.
            var porCategoria = await Filtrar(publicados, filtro, categoria: false)
                .GroupBy(c => c.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.CategoriaId, g => g.Cantidad);
            var categorias = (await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync())
                .Select(c => new CategoriaConteo { Id = c.Id, Nombre = c.Nombre, Cantidad = porCategoria.GetValueOrDefault(c.Id) })
                .Where(c => c.Cantidad > 0 || c.Id == filtro.CategoriaId)
                .ToList();

            var porNivel = await Filtrar(publicados, filtro, nivel: false)
                .GroupBy(c => c.Nivel)
                .Select(g => new { Nivel = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.Nivel, g => g.Cantidad);

            var sinDuracion = Filtrar(publicados, filtro, duracion: false);
            var porDuracion = new Dictionary<string, int>
            {
                ["corta"] = await sinDuracion.CountAsync(c => c.DuracionMinutos < DuracionCorta),
                ["media"] = await sinDuracion.CountAsync(c => c.DuracionMinutos >= DuracionCorta && c.DuracionMinutos <= DuracionMedia),
                ["larga"] = await sinDuracion.CountAsync(c => c.DuracionMinutos > DuracionMedia)
            };

            var cursos = Filtrar(publicados, filtro).Include(c => c.Categoria);

            var total = await cursos.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)CursosPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var ordenados = filtro.Orden switch
            {
                "titulo" => cursos.OrderBy(c => c.Titulo).ThenBy(c => c.Id),
                "duracion" => cursos.OrderBy(c => c.DuracionMinutos).ThenBy(c => c.Titulo),
                _ => cursos.OrderByDescending(c => c.FechaCreacion).ThenBy(c => c.Id)
            };
            var lista = await ordenados
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
                Filtro = filtro,
                Categorias = categorias,
                ConteoNiveles = porNivel,
                ConteoDuraciones = porDuracion,
                Total = total,
                Pagina = pagina,
                TotalPaginas = totalPaginas
            });
        }

        private static IQueryable<Curso> Filtrar(IQueryable<Curso> cursos, FiltroCatalogo filtro,
            bool categoria = true, bool nivel = true, bool duracion = true)
        {
            if (categoria && filtro.CategoriaId != null)
            {
                cursos = cursos.Where(c => c.CategoriaId == filtro.CategoriaId);
            }
            if (nivel && filtro.Niveles.Count > 0)
            {
                var niveles = filtro.Niveles.Cast<NivelCurso>().ToList();
                cursos = cursos.Where(c => niveles.Contains(c.Nivel));
            }
            if (duracion)
            {
                cursos = filtro.Duracion switch
                {
                    "corta" => cursos.Where(c => c.DuracionMinutos < DuracionCorta),
                    "media" => cursos.Where(c => c.DuracionMinutos >= DuracionCorta && c.DuracionMinutos <= DuracionMedia),
                    "larga" => cursos.Where(c => c.DuracionMinutos > DuracionMedia),
                    _ => cursos
                };
            }
            return cursos;
        }
        [AllowAnonymous]
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
