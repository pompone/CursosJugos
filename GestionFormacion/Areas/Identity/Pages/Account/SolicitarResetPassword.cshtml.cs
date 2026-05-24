using GestionFormacion.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GestionFormacion.Areas.Identity.Pages.Account
{
    public class SolicitarResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SolicitarResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? Mensaje { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Legajo")]
            public string Legajo { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var usuario = await _userManager.FindByNameAsync(Input.Legajo);

            if (usuario == null)
            {
                Mensaje = "Si el legajo ingresado existe, la solicitud será revisada por el administrador.";
                return Page();
            }

            usuario.SolicitoResetPassword = true;
            usuario.FechaSolicitudReset = DateTime.Now;

            await _userManager.UpdateAsync(usuario);

            Mensaje = "Solicitud enviada correctamente. El administrador deberá aprobar el reseteo.";

            return Page();
        }
    }
}