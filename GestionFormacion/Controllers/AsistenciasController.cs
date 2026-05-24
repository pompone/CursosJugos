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
    public class AsistenciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Asistencias
        public async Task<IActionResult> Index()
        {
            var consulta = _context.Asistencias
                .Include(a => a.Curso)
                .Include(a => a.Empleado)
                .AsQueryable();

            if (User.IsInRole("Empleado"))
            {
                string legajo = User.Identity?.Name;

                consulta = consulta.Where(a => a.Empleado.Legajo == legajo);
            }

            return View(await consulta.ToListAsync());
        }

        // GET: Asistencias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistencia = await _context.Asistencias
                .Include(a => a.Curso)
                .Include(a => a.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asistencia == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Empleado") &&
                asistencia.Empleado.Legajo != User.Identity?.Name)
            {
                return Forbid();
            }

            return View(asistencia);
        }

        // GET: Asistencias/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "NombreCurso");
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Apellido");

            return View();
        }

        // POST: Asistencias/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,EmpleadoId,CursoId,Fecha,Asistio")] Asistencia asistencia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asistencia);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "NombreCurso", asistencia.CursoId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Apellido", asistencia.EmpleadoId);

            return View(asistencia);
        }

        // GET: Asistencias/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistencia = await _context.Asistencias.FindAsync(id);

            if (asistencia == null)
            {
                return NotFound();
            }

            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "NombreCurso", asistencia.CursoId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Apellido", asistencia.EmpleadoId);

            return View(asistencia);
        }

        // POST: Asistencias/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,EmpleadoId,CursoId,Fecha,Asistio")] Asistencia asistencia)
        {
            if (id != asistencia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asistencia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsistenciaExists(asistencia.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CursoId"] = new SelectList(_context.Cursos, "Id", "NombreCurso", asistencia.CursoId);
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Apellido", asistencia.EmpleadoId);

            return View(asistencia);
        }

        // GET: Asistencias/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistencia = await _context.Asistencias
                .Include(a => a.Curso)
                .Include(a => a.Empleado)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asistencia == null)
            {
                return NotFound();
            }

            return View(asistencia);
        }

        // POST: Asistencias/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asistencia = await _context.Asistencias.FindAsync(id);

            if (asistencia != null)
            {
                _context.Asistencias.Remove(asistencia);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AsistenciaExists(int id)
        {
            return _context.Asistencias.Any(e => e.Id == id);
        }
    }
}