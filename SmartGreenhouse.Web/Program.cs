using SmartGreenhouse.Web.Services;
using SmartGreenhouse.Web.Models;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. РЕЄСТРАЦІЯ СЕРВІСІВ
// =========================================================

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<ISensorService, SensorService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=SmartGreenhouse.db"));

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
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
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
    SeedData.Initialize(context);   // ← вот здесь правильно
}

// =========================================================
// 4. МАРШРУТИЗАЦІЯ
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.Run();
