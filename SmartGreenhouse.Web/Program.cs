using Microsoft.AspNetCore.Authentication.Cookies;
using SmartGreenhouse.Web.Services;
using SmartGreenhouse.Web.Models; // Потрібно, якщо якісь моделі використовуються тут
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data; // тут буде AppDbContext
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. РЕЄСТРАЦІЯ СЕРВІСІВ (Dependency Injection)
// =========================================================

// Додаємо MVC контролери та Views
builder.Services.AddControllersWithViews();

// [ВАЖЛИВО] Реєструємо сховище користувачів (Singleton = одна база в пам'яті на всіх)
builder.Services.AddSingleton<UserStore>();

// [ВАЖЛИВО] Реєструємо сервіс теплиці (НЕ одна теплиця на всіх)
builder.Services.AddSingleton<ISensorService, SensorService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=SmartGreenhouse.db"));

// =========================================================
// 2. НАЛАШТУВАННЯ АВТОРИЗАЦІЇ ТА СЕСІЙ
// =========================================================

// Налаштування Cookies (Сучасний спосіб входу)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Якщо незалогінений юзер лізе в теплицю -> кидаємо його на логін
    options.LoginPath = "/Auth/Login"; 
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddGoogle(options =>
{
    // Беремо ClientId та ClientSecret з appsettings.json
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google"; // стандартний шлях для Google OAuth
});

// Налаштування Сесій (Для сумісності зі старим кодом Subroutine1/2)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =========================================================
// 3. НАЛАШТУВАННЯ PIPELINE (Обробка запитів)
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Дозволяє CSS, картинки

app.UseRouting();

// Порядок важливий!
app.UseSession();        // 1. Включаємо сесію
app.UseAuthentication(); // 2. Перевіряємо "хто це" (Login)
app.UseAuthorization();  // 3. Перевіряємо "чи можна" (Access)

// =========================================================
// 4. МАРШРУТИЗАЦІЯ (Routing)
// =========================================================

app.MapControllerRoute(
    name: "default",
    // ТУТ ЗМІНА: Стартуємо з HomeController, метод Welcome
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.Run();
