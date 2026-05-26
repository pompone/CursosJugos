
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionFormacion.Data;
using GestionFormacion.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestionFormacion.Controllers
{
    [Authorize]
    public class InscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscripcionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? estado, string? busqueda)
        {
            var consulta = _context.Inscripciones
                .Include(i => i.Curso)
                .Include(i => i.Empleado)
                .AsQueryable();

            if (User.IsInRole("Empleado"))
            {
                string? legajo = User.Identity?.Name;
                consulta = consulta.Where(i => i.Empleado.Legajo == legajo);
            }

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(i => i.Estado == estado);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                consulta = consulta.Where(i =>
                    i.Empleado.Nombre.Contains(busqueda) ||
                    i.Empleado.Apellido.Contains(busqueda) ||
                    i.Curso.NombreCurso.Contains(busqueda)
                );
            }

            return View(await consulta.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inscripcion = await _context.Inscripciones
                .Include(i => i.Curso)
                .Include(i => i.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inscripcion == null) return NotFound();

            if (User.IsInRole("Empleado") &&
                inscripcion.Empleado.Legajo != User.Identity?.Name)
            {
                return Forbid();
            }

            return View(inscripcion);
        }

        [Authorize(Roles = "Empleado")]
        [HttpGet]
        public async Task<IActionResult> Create(int? cursoId)
        {
            if (cursoId == null)
                return RedirectToAction("Index", "Cursos");

            var curso = await _context.Cursos.FindAsync(cursoId);

            if (curso == null || !curso.Activo)
                return NotFound();

            var inscripcion = new Inscripcion
            {
                CursoId = curso.Id,
                Curso = curso,
                FechaInscripcion = DateTime.UtcNow,
                Estado = "Pendiente"
            };

            return View(inscripcion);
        }

        [Authorize(Roles = "Empleado")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CursoId")] Inscripcion inscripcion)
        {
            string? legajo = User.Identity?.Name;

            if (string.IsNullOrEmpty(legajo))
                return Unauthorized();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(e => e.Legajo == legajo && e.Activo);

            if (empleado == null)
            {
                ModelState.AddModelError("", "No se encontró un empleado activo asociado al legajo del usuario.");
                return View(inscripcion);
            }

            var curso = await _context.Cursos.FindAsync(inscripcion.CursoId);

            if (curso == null || !curso.Activo)
                return NotFound();

            bool yaExiste = await _context.Inscripciones.AnyAsync(i =>
                i.EmpleadoId == empleado.Id &&
                i.CursoId == inscripcion.CursoId
            );

            if (yaExiste)
            {
                ModelState.AddModelError("", "Ya solicitaste la inscripción a este curso.");
                inscripcion.Curso = curso;
                return View(inscripcion);
            }

            inscripcion.EmpleadoId = empleado.Id;
            inscripcion.FechaInscripcion = DateTime.UtcNow;
            inscripcion.Estado = "Pendiente";

            _context.Add(inscripcion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inscripcion = await _context.Inscripciones.FindAsync(id);

            if (inscripcion == null) return NotFound();

            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Apellido", inscripcion.EmpleadoId);
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "NombreCurso", inscripcion.CursoId);

            return View(inscripcion);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int EmpleadoId, int CursoId, string Estado)
        {
            var inscripcionExistente = await _context.Inscripciones.FindAsync(id);

            if (inscripcionExistente == null)
                return NotFound();

            try
            {
                inscripcionExistente.EmpleadoId = EmpleadoId;
                inscripcionExistente.CursoId = CursoId;
                inscripcionExistente.Estado = Estado;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InscripcionExists(id))
                    return NotFound();

                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inscripcion = await _context.Inscripciones
                .Include(i => i.Curso)
                .Include(i => i.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inscripcion == null) return NotFound();

            return View(inscripcion);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);

            if (inscripcion != null)
                _context.Inscripciones.Remove(inscripcion);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);

            if (inscripcion == null)
                return NotFound();

            inscripcion.Estado = "Aprobada";

            _context.Update(inscripcion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);

            if (inscripcion == null)
                return NotFound();

            inscripcion.Estado = "Rechazada";

            _context.Update(inscripcion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool InscripcionExists(int id)
        {
            return _context.Inscripciones.Any(e => e.Id == id);
        }
    }
}
