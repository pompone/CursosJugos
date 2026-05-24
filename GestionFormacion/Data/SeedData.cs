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

            string usuarioAdmin = "ADMIN";
            string emailAdmin = "admin@gestionformacion.local";
            string passwordAdmin = "Admin123!";

            var admin = await userManager.FindByNameAsync(usuarioAdmin);

            if (admin != null)
            {
                await userManager.DeleteAsync(admin);
            }

            admin = new ApplicationUser
            {
                UserName = usuarioAdmin,
                Email = emailAdmin,
                EmailConfirmed = true,
                DebeCambiarPassword = false,
                SolicitoResetPassword = false,
                FechaSolicitudReset = null
            };

            var resultadoAdmin = await userManager.CreateAsync(admin, passwordAdmin);

            if (resultadoAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            string legajoEmpleado = "1001";
            string emailEmpleado = "empleado1001@gestionformacion.local";
            string passwordEmpleado = "Empleado123!";

            var empleado = await userManager.FindByNameAsync(legajoEmpleado);

            if (empleado == null)
            {
                empleado = new ApplicationUser
                {
                    UserName = legajoEmpleado,
                    Email = emailEmpleado,
                    EmailConfirmed = true,
                    DebeCambiarPassword = true
                };

                await userManager.CreateAsync(empleado, passwordEmpleado);
                await userManager.AddToRoleAsync(empleado, "Empleado");
            }
        }
    }
}