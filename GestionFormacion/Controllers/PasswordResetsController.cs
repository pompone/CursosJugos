using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestionFormacion.Data;
using GestionFormacion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestionFormacion.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PasswordResetsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PasswordResetsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var usuarios = _userManager.Users
                .Where(u => u.SolicitoResetPassword)
                .ToList();

            var modelo = new List<PasswordResetViewModel>();

            foreach (var usuario in usuarios)
            {
                var empleado = _context.Empleados
                    .FirstOrDefault(e => e.Legajo == usuario.UserName);

                modelo.Add(new PasswordResetViewModel
                {
                    Id = usuario.Id,
                    Legajo = usuario.UserName ?? "",
                    NombreCompleto = empleado != null
                        ? $"{empleado.Apellido}, {empleado.Nombre}"
                        : "Empleado no encontrado",
                    FechaSolicitudReset = usuario.FechaSolicitudReset
                });
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            string passwordTemporal = "Inicial123!";

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            var resultado = await _userManager.ResetPasswordAsync(
                usuario,
                token,
                passwordTemporal
            );

            if (resultado.Succeeded)
            {
                usuario.SolicitoResetPassword = false;
                usuario.FechaSolicitudReset = null;
                usuario.DebeCambiarPassword = true;

                await _userManager.UpdateAsync(usuario);

                TempData["Mensaje"] =
                    $"Contraseña reseteada correctamente. Usuario: {usuario.UserName} - Contraseña temporal: {passwordTemporal}";
            }
            else
            {
                TempData["Mensaje"] = "No se pudo resetear la contraseña.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Denegar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.SolicitoResetPassword = false;
            usuario.FechaSolicitudReset = null;

            await _userManager.UpdateAsync(usuario);

            TempData["Mensaje"] =
                $"Solicitud de reseteo denegada para el usuario {usuario.UserName}.";

            return RedirectToAction(nameof(Index));
        }
    }

    public class PasswordResetViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime? FechaSolicitudReset { get; set; }
    }
}