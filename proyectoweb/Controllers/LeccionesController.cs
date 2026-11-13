using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using proyectoweb.Data;
using proyectoweb.Models;

namespace proyectoweb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LeccionesController : Controller
    {
        private readonly AppDbContext _context;

        public LeccionesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? cursoId)
        {
            var lecciones = _context.Lecciones.Include(l => l.Curso).AsQueryable();
            if (cursoId != null)
            {
                lecciones = lecciones.Where(l => l.CursoId == cursoId);
                ViewData["Curso"] = await _context.Cursos.FindAsync(cursoId);
            }

            return View(await lecciones.OrderBy(l => l.CursoId).ThenBy(l => l.Orden).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leccion == null)
            {
                return NotFound();
            }

            return View(leccion);
        }

        public async Task<IActionResult> Create(int? cursoId)
        {
            var leccion = new Leccion();
            if (cursoId != null)
            {
                leccion.CursoId = cursoId.Value;
                leccion.Orden = await SiguienteOrden(cursoId.Value);
            }

            ViewData["CursoId"] = ListaCursos(cursoId);
            return View(leccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leccion);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Lección creada.";
                return RedirectToAction(nameof(Index), new { cursoId = leccion.CursoId });
            }
            ViewData["CursoId"] = ListaCursos(leccion.CursoId);
            return View(leccion);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null)
            {
                return NotFound();
            }
            ViewData["CursoId"] = ListaCursos(leccion.CursoId);
            return View(leccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Contenido,Orden,CursoId")] Leccion leccion)
        {
            if (id != leccion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leccion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeccionExists(leccion.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                TempData["Exito"] = "Lección actualizada.";
                return RedirectToAction(nameof(Index), new { cursoId = leccion.CursoId });
            }
            ViewData["CursoId"] = ListaCursos(leccion.CursoId);
            return View(leccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mover(int id, int direccion)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null)
            {
                return NotFound();
            }

            var delCurso = _context.Lecciones.Where(l => l.CursoId == leccion.CursoId);
            var vecina = direccion < 0
                ? await delCurso.Where(l => l.Orden < leccion.Orden).OrderByDescending(l => l.Orden).FirstOrDefaultAsync()
                : await delCurso.Where(l => l.Orden > leccion.Orden).OrderBy(l => l.Orden).FirstOrDefaultAsync();

            if (vecina != null)
            {
                (leccion.Orden, vecina.Orden) = (vecina.Orden, leccion.Orden);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { cursoId = leccion.CursoId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leccion == null)
            {
                return NotFound();
            }

            ViewData["Completadas"] = await _context.LeccionesCompletadas.CountAsync(lc => lc.LeccionId == leccion.Id);
            return View(leccion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.Lecciones.Remove(leccion);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Lección eliminada.";
            return RedirectToAction(nameof(Index), new { cursoId = leccion.CursoId });
        }

        private SelectList ListaCursos(int? seleccionado = null)
        {
            return new SelectList(_context.Cursos.OrderBy(c => c.Titulo), "Id", "Titulo", seleccionado);
        }

        private async Task<int> SiguienteOrden(int cursoId)
        {
            var maximo = await _context.Lecciones
                .Where(l => l.CursoId == cursoId)
                .MaxAsync(l => (int?)l.Orden);
            return (maximo ?? 0) + 1;
        }

        private bool LeccionExists(int id)
        {
            return _context.Lecciones.Any(e => e.Id == id);
        }
    }
}
