using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OK.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios para usar sesiones
builder.Services.AddDistributedMemoryCache(); // Necesario para almacenar la sesi�n en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Configura el tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true; // Establece la cookie como HttpOnly
    options.Cookie.IsEssential = true; // Hace la cookie esencial
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login"; // P�gina de login
                    options.LogoutPath = "/Logout"; // P�gina de logout
                });


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Add services to the container.
builder.Services.AddControllersWithViews();



var app = builder.Build();

// Usar el middleware de sesi�n
app.UseSession(); // Necesario para habilitar la sesi�n

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
