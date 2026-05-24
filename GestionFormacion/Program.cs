using GestionFormacion.Data;
using GestionFormacion.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// PostgreSQL / Supabase
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// En Render no conviene forzar redirección HTTPS desde la app.
// Render ya maneja HTTPS externamente.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint manual para crear el usuario administrador.
// Usarlo una sola vez después del deploy.
app.MapGet("/init-db", async (
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) =>
{
    try
    {
        // 1. Verificar/crear rol Admin
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var resultadoRol = await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!resultadoRol.Succeeded)
            {
                return Results.BadRequest(resultadoRol.Errors);
            }
        }

        // 2. Verificar/crear usuario ADMIN
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

            var resultadoUsuario = await userManager.CreateAsync(admin, "Admin123!");

            if (!resultadoUsuario.Succeeded)
            {
                return Results.BadRequest(resultadoUsuario.Errors);
            }
        }

        // 3. Verificar/asignar rol Admin al usuario
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var resultadoRolUsuario = await userManager.AddToRoleAsync(admin, "Admin");

            if (!resultadoRolUsuario.Succeeded)
            {
                return Results.BadRequest(resultadoRolUsuario.Errors);
            }
        }

        return Results.Ok("Usuario ADMIN creado correctamente.");
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error al crear usuario ADMIN",
            detail: ex.ToString()
        );
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
