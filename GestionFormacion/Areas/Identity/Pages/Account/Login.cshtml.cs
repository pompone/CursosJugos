#nullable disable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GestionFormacion.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GestionFormacion.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Legajo")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var usuario = await _userManager.FindByNameAsync(Input.UserName);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Legajo o contraseña incorrectos.");
                return Page();
            }

            var passwordCorrecta = await _userManager.CheckPasswordAsync(usuario, Input.Password);

            if (!passwordCorrecta)
            {
                ModelState.AddModelError(string.Empty, "Legajo o contraseña incorrectos.");
                return Page();
            }

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                await _signInManager.CreateUserPrincipalAsync(usuario));

            _logger.LogInformation("Usuario inició sesión.");

            if (usuario.DebeCambiarPassword)
            {
                return RedirectToPage("./CambiarPasswordInicial");
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}