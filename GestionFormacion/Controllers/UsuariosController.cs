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
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string busqueda)
        {
            var usuarios = _userManager.Users
                .ToList()
                .Where(u =>
                    u.UserName == "ADMIN" ||
                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.All(char.IsDigit))
                )
                .ToList();

            var modelo = new List<UsuarioRolViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                var empleado = _context.Empleados
                    .FirstOrDefault(e => e.Legajo == usuario.UserName);

                modelo.Add(new UsuarioRolViewModel
                {
                    Id = usuario.Id,
                    UserName = usuario.UserName,
                    NombreCompleto = empleado != null
                        ? $"{empleado.Apellido}, {empleado.Nombre}"
                        : usuario.UserName == "ADMIN"
                            ? "Administrador del sistema"
                            : "Empleado no encontrado",
                    Roles = string.Join(", ", roles)
                });
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                modelo = modelo
                    .Where(u =>
                        (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(busqueda)) ||
                        u.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HacerAdmin(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _userManager.IsInRoleAsync(usuario, "Admin"))
            {
                await _userManager.AddToRoleAsync(usuario, "Admin");
                TempData["Mensaje"] = $"El usuario {usuario.UserName} ahora tiene rol Admin.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarAdmin(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.UserName == "ADMIN")
            {
                TempData["Mensaje"] = "No se puede quitar el rol Admin al usuario principal.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(usuario, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(usuario, "Admin");
                TempData["Mensaje"] = $"El usuario {usuario.UserName} volvió a tener solo rol Empleado.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class UsuarioRolViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
    }
}