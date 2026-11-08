using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proyectoweb.Data;
using proyectoweb.Models;

namespace proyectoweb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CursosController : Controller
    {
        private readonly AppDbContext _context;

        public CursosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cursos = _context.Cursos
                .Include(c => c.Categoria)
                .Include(c => c.Inscripciones)
                .OrderBy(c => c.Titulo);
            return View(await cursos.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .Include(c => c.Categoria)
                .Include(c => c.Lecciones!.OrderBy(l => l.Orden))
                .FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        public IActionResult Create()
        {
            ViewData["CategoriaId"] = ListaCategorias();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Descripcion,Nivel,DuracionMinutos,ImagenUrl,Publicado,CategoriaId")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = ListaCategorias(curso.CategoriaId);
            return View(curso);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = ListaCategorias(curso.CategoriaId);
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descripcion,Nivel,DuracionMinutos,ImagenUrl,Publicado,FechaCreacion,CategoriaId")] Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = ListaCategorias(curso.CategoriaId);
            return View(curso);
        }

        public async Task<IActionResult> Inscritos(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            var inscritos = await _context.Inscripciones
                .Where(i => i.CursoId == id)
                .OrderBy(i => i.FechaInscripcion)
                .Select(i => new InscritoCurso
                {
                    Email = i.User!.Email,
                    FechaInscripcion = i.FechaInscripcion,
                    FechaFinalizacion = i.FechaFinalizacion,
                    Completadas = _context.LeccionesCompletadas
                        .Count(lc => lc.UserId == i.UserId && lc.Leccion!.CursoId == id)
                })
                .ToListAsync();

            ViewData["Curso"] = curso;
            ViewData["TotalLecciones"] = await _context.Lecciones.CountAsync(l => l.CursoId == id);
            return View(inscritos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPublicacion(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Publicado = !curso.Publicado;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .Include(c => c.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private SelectList ListaCategorias(int? seleccionada = null)
        {
            return new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", seleccionada);
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }
    }
}
