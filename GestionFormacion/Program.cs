using GestionFormacion.Data;
using GestionFormacion.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
        ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.InicializarRolesYUsuarios(scope.ServiceProvider);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/crear-admin", async (
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) =>
{
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
            SolicitoResetPassword = false,
            FechaSolicitudReset = null
        };

        var resultadoAdmin = await userManager.CreateAsync(admin, "Admin123!");

        if (resultadoAdmin.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    var empleado = await userManager.FindByNameAsync("1001");

    if (empleado == null)
    {
        empleado = new ApplicationUser
        {
            UserName = "1001",
            Email = "empleado1001@gestionformacion.local",
            EmailConfirmed = true,
            DebeCambiarPassword = true,
            SolicitoResetPassword = false,
            FechaSolicitudReset = null
        };

        var resultadoEmpleado = await userManager.CreateAsync(empleado, "Empleado123!");

        if (resultadoEmpleado.Succeeded)
        {
            await userManager.AddToRoleAsync(empleado, "Empleado");
        }
    }

    return Results.Ok("Usuarios y roles creados correctamente.");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
