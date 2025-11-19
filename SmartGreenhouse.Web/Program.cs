using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using SmartGreenhouse.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Додаємо наш сервіс UserStore
builder.Services.AddSingleton<UserStore>();

// Налаштування аутентифікації
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // маршрут для логіну
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    // Введи свої ClientId/ClientSecret у appsettings.json або змінні оточення
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // middleware аутентифікації
app.UseAuthorization();  // middleware авторизації

// Маршрути
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

// Додатковий маршрут для Subroutine (задач)
app.MapControllerRoute(
    name: "subroutine",
    pattern: "Subroutine/{action=Index}/{task?}",
    defaults: new { controller = "Subroutine" });

// Маршрути для Auth (Login/Logout/Register)
app.MapControllerRoute(
    name: "auth",
    pattern: "Auth/{action=Login}/{id?}",
    defaults: new { controller = "Auth" });

app.Run();
