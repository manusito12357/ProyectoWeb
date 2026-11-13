using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proyectoweb.Data;
using proyectoweb.Models;

namespace proyectoweb.Controllers
{
    [Authorize(Roles = "Suscriptor")]
    public class MisCursosController : Controller
    {
        private readonly AppDbContext _context;

        public MisCursosController(AppDbContext context)
        {
            _context = context;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index()
        {
            var userId = UserId;
            var cursos = await _context.Inscripciones
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.FechaInscripcion)
                .Select(i => new ProgresoCurso
                {
                    Curso = i.Curso!,
                    Total = i.Curso!.Lecciones!.Count,
                    Completadas = _context.LeccionesCompletadas
                        .Count(lc => lc.UserId == userId && lc.Leccion!.CursoId == i.CursoId)
                })
                .ToListAsync();

            return View(cursos);
        }

        public async Task<IActionResult> Curso(int id)
        {
            if (!await _context.EstaInscrito(UserId, id))
            {
                return RedirectToAction("Detalles", "Catalogo", new { id });
            }

            var curso = await _context.Cursos
                .Include(c => c.Lecciones!.OrderBy(l => l.Orden))
                .FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            var completadas = await LeccionesCompletadasDe(id);
            ViewData["Completadas"] = completadas;

            return View(new ProgresoCurso
            {
                Curso = curso,
                Total = curso.Lecciones!.Count,
                Completadas = completadas.Count
            });
        }

        public async Task<IActionResult> Leccion(int id)
        {
            var leccion = await _context.Lecciones.Include(l => l.Curso).FirstOrDefaultAsync(l => l.Id == id);
            if (leccion == null)
            {
                return NotFound();
            }

            if (!await _context.EstaInscrito(UserId, leccion.CursoId))
            {
                return Forbid();
            }

            var ids = await _context.Lecciones
                .Where(l => l.CursoId == leccion.CursoId)
                .OrderBy(l => l.Orden)
                .Select(l => l.Id)
                .ToListAsync();
            var posicion = ids.IndexOf(id);

            ViewData["Posicion"] = posicion + 1;
            ViewData["Total"] = ids.Count;
            ViewData["Anterior"] = posicion > 0 ? ids[posicion - 1] : (int?)null;
            ViewData["Siguiente"] = posicion < ids.Count - 1 ? ids[posicion + 1] : (int?)null;
            ViewData["Completada"] = (await LeccionesCompletadasDe(leccion.CursoId)).Contains(id);

            return View(leccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Completar(int id)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null)
            {
                return NotFound();
            }

            var userId = UserId;
            if (!await _context.EstaInscrito(userId, leccion.CursoId))
            {
                return Forbid();
            }

            var yaCompletada = await _context.LeccionesCompletadas
                .AnyAsync(lc => lc.UserId == userId && lc.LeccionId == id);
            if (!yaCompletada)
            {
                _context.LeccionesCompletadas.Add(new LeccionCompletada { UserId = userId, LeccionId = id });
                await _context.SaveChangesAsync();
                await MarcarFinalizacion(userId, leccion.CursoId);
            }

            return RedirectToAction(nameof(Leccion), new { id });
        }

        private async Task<HashSet<int>> LeccionesCompletadasDe(int cursoId)
        {
            var userId = UserId;
            var ids = await _context.LeccionesCompletadas
                .Where(lc => lc.UserId == userId && lc.Leccion!.CursoId == cursoId)
                .Select(lc => lc.LeccionId)
                .ToListAsync();
            return ids.ToHashSet();
        }

        private async Task MarcarFinalizacion(string userId, int cursoId)
        {
            var total = await _context.Lecciones.CountAsync(l => l.CursoId == cursoId);
            var completadas = (await LeccionesCompletadasDe(cursoId)).Count;
            if (total == 0 || completadas < total)
            {
                return;
            }

            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.UserId == userId && i.CursoId == cursoId);
            if (inscripcion != null && inscripcion.FechaFinalizacion == null)
            {
                inscripcion.FechaFinalizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
