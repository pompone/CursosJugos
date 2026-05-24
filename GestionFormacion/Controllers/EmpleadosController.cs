using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFormacion.Data;
using GestionFormacion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GestionFormacion.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmpleadosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmpleadosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Empleados.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Legajo,Nombre,Apellido,Dni,Sector,Puesto,Activo")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                bool existeEmpleado = await _context.Empleados
                    .AnyAsync(e => e.Legajo == empleado.Legajo);

                if (existeEmpleado)
                {
                    ModelState.AddModelError("Legajo", "Ya existe un empleado con este legajo.");
                    return View(empleado);
                }

                var usuarioExistente = await _userManager.FindByNameAsync(empleado.Legajo);

                if (usuarioExistente != null)
                {
                    ModelState.AddModelError("Legajo", "Ya existe un usuario con este legajo.");
                    return View(empleado);
                }

                _context.Add(empleado);
                await _context.SaveChangesAsync();

                var usuario = new ApplicationUser
                {
                    UserName = empleado.Legajo,
                    Email = $"empleado{empleado.Legajo}@gestionformacion.local",
                    EmailConfirmed = true,
                    DebeCambiarPassword = true
                };

                string passwordTemporal = "Inicial123!";

                var resultado = await _userManager.CreateAsync(usuario, passwordTemporal);

                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, "Empleado");

                    TempData["Mensaje"] =
                        $"Empleado creado correctamente. Usuario: {empleado.Legajo} - Contraseña temporal: {passwordTemporal}";

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(empleado);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FindAsync(id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Legajo,Nombre,Apellido,Dni,Sector,Puesto,Activo")] Empleado empleado)
        {
            if (id != empleado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                bool legajoDuplicado = await _context.Empleados
                    .AnyAsync(e => e.Legajo == empleado.Legajo && e.Id != empleado.Id);

                if (legajoDuplicado)
                {
                    ModelState.AddModelError("Legajo", "Ya existe otro empleado con este legajo.");
                    return View(empleado);
                }

                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id)) return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(empleado);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);

            if (empleado != null)
            {
                var usuario = await _userManager.FindByNameAsync(empleado.Legajo);

                if (usuario != null)
                {
                    await _userManager.DeleteAsync(usuario);
                }

                _context.Empleados.Remove(empleado);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }
}