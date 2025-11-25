using SmartGreenhouse.Web.Services;
using SmartGreenhouse.Web.Models;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. РЕЄСТРАЦІЯ СЕРВІСІВ ТА ВИБІР БД
// =========================================================

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<ISensorService, SensorService>();

// ---> ДОДАНО: Реєстрація фонового генератора даних <---
// Цей сервіс працюватиме незалежно від дій користувача
builder.Services.AddHostedService<DataGeneratorService>();

// Отримуємо тип провайдера з конфігурації (appsettings.json)
var dbProvider = builder.Configuration["DatabaseProvider"];
var connectionString = builder.Configuration.GetConnectionString(dbProvider);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (dbProvider)
    {
        case "Sqlite":
            options.UseSqlite(connectionString);
            break;
        case "SqlServer":
            options.UseSqlServer(connectionString);
            break;
        case "Postgres":
            // Для PostgreSQL може знадобитися увімкнення legacy timestamp behavior
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            options.UseNpgsql(connectionString);
            break;
        case "InMemory":
            options.UseInMemoryDatabase(connectionString ?? "GreenhouseTestDb");
            break;
        default:
            throw new Exception($"Unsupported database provider: {dbProvider}");
    }
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login"; 
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddGoogle(options =>
{
    // Перевірка на null, щоб не падало при відсутності ключів
    var googleAuth = builder.Configuration.GetSection("Authentication:Google");
    if (googleAuth.Exists())
    {
        options.ClientId = googleAuth["ClientId"];
        options.ClientSecret = googleAuth["ClientSecret"];
        options.CallbackPath = "/signin-google";
    }
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =========================================================
// 2. PIPELINE
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// 3. ІНІЦІАЛІЗАЦІЯ БД (SeedData)
// =========================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Викликаємо наш новий ініціалізатор
    SeedData.Initialize(context); 
}

// =========================================================
// 4. МАРШРУТИЗАЦІЯ
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.Run();