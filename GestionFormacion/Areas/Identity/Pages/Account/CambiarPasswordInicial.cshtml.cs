using GestionFormacion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GestionFormacion.Areas.Identity.Pages.Account
{
    [Authorize]
    public class CambiarPasswordInicialModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CambiarPasswordInicialModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña actual")]
            public string PasswordActual { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Nueva contraseña")]
            public string PasswordNueva { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("PasswordNueva")]
            [Display(Name = "Confirmar contraseña")]
            public string ConfirmarPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return RedirectToPage("./Login");
            }

            if (!usuario.DebeCambiarPassword)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return RedirectToPage("./Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resultado = await _userManager.ChangePasswordAsync(
                usuario,
                Input.PasswordActual,
                Input.PasswordNueva
            );

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            usuario.DebeCambiarPassword = false;

            await _userManager.UpdateAsync(usuario);

            await _signInManager.RefreshSignInAsync(usuario);

            return RedirectToPage("/Index");
        }
    }
}