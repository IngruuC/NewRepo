using MODELO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using Npgsql.EntityFrameworkCore.PostgreSQL;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Contexto>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// ── Render.com usa proxy inverso — confiar en TODOS los proxies ──
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // CRÍTICO: limpiar listas para aceptar cualquier proxy (Render, Cloudflare, etc.)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    // Render puede tener múltiples proxies en cadena — procesar todos
    options.ForwardLimit = null;
});

builder.Services.AddScoped<PROYECTO_WEB.TESIS.Services.AfipService>();

var app = builder.Build();

// DEBE IR PRIMERO, antes que todo
app.UseForwardedHeaders();

// Migración y seed inicial
using (var scope = app.Services.CreateScope())
{
    try
    {
        var contexto = scope.ServiceProvider.GetRequiredService<Contexto>();
        contexto.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al migrar: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();