using GestionFormacion.Models;
using Microsoft.AspNetCore.Identity;

namespace GestionFormacion.Data
{
    public static class SeedData
    {
        public static async Task InicializarRolesYUsuarios(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Empleado" };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            var admin = await userManager.FindByNameAsync("ADMIN");

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "ADMIN",
                    Email = "admin@gestionformacion.local",
                    EmailConfirmed = true,
                    DebeCambiarPassword = false,
                    SolicitoResetPassword = false
                };

                var resultado = await userManager.CreateAsync(admin, "Admin123!");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
