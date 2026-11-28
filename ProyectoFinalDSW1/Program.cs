using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalDSW1.Data;

var builder = WebApplication.CreateBuilder(args);

// En desarrollo se permite http; fuera de desarrollo las cookies solo viajan por https
var politicaCookie = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Todas las pantallas exigen sesión iniciada (excepto las marcadas con [AllowAnonymous])
    var politicaSesion = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(politicaSesion));

    // Protección contra CSRF en todos los formularios POST
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Evita que el navegador guarde las pantallas (botón "atrás" después de cerrar sesión)
    options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<TallerRepository>();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = politicaCookie;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TallerAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = politicaCookie;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        // En cada petición se confirma que el usuario sigue activo y con el mismo rol
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async contexto =>
            {
                var idTexto = contexto.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var rolClaim = contexto.Principal?.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(idTexto, out var id))
                {
                    contexto.RejectPrincipal();
                    await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var db = contexto.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var usuario = await db.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == id)
                    .Select(u => new { u.Activo, u.Rol })
                    .FirstOrDefaultAsync();

                if (usuario == null || !usuario.Activo || usuario.Rol.ToString() != rolClaim)
                {
                    contexto.RejectPrincipal();
                    await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    });

var app = builder.Build();

// Crea la base de datos, los procedimientos y los usuarios iniciales
await DbInitializer.InicializarAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Cabeceras de seguridad
app.Use(async (contexto, siguiente) =>
{
    var cabeceras = contexto.Response.Headers;
    cabeceras["X-Content-Type-Options"] = "nosniff";
    cabeceras["X-Frame-Options"] = "DENY";
    cabeceras["Referrer-Policy"] = "no-referrer";
    cabeceras["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    if (!app.Environment.IsDevelopment())
    {
        cabeceras["Content-Security-Policy"] =
            "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self'; frame-ancestors 'none'; form-action 'self'";
    }
    await siguiente();
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
