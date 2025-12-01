using SmartGreenhouse.Web.Services;
using SmartGreenhouse.Web.Models;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;


var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. РЕЄСТРАЦІЯ СЕРВІСІВ ТА ВИБІР БД
// =========================================================


builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<ISensorService, SensorService>();
builder.Services.AddHttpClient(); // Додаємо HttpClient


builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Реєстрація V1 клієнта (Сумісний інтерфейс)
builder.Services.AddScoped<SmartGreenhouse.Clients.ApiV1Client>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    // ВИКОРИСТОВУЄМО ПОРТ 5273
    return new SmartGreenhouse.Clients.ApiV1Client("http://localhost:5273", httpClient); 
});

// Реєстрація V2 клієнта (Розширений функціонал)
builder.Services.AddScoped<SmartGreenhouse.Clients.ApiV2Client>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    // ВИКОРИСТОВУЄМО ПОРТ 5273
    return new SmartGreenhouse.Clients.ApiV2Client("http://localhost:5273", httpClient);
});

// Фоновий генератор даних
builder.Services.AddHostedService<DataGeneratorService>();

// Налаштування БД
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

// =========================================================
// 2. АВТОРИЗАЦІЯ ТА АУТЕНТИФІКАЦІЯ
// =========================================================

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

// =========================================================
// 3. API VERSIONING
// =========================================================

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // v1, v2
    options.SubstituteApiVersionInUrl = true;
});

// =========================================================
// 4. SWAGGER
// =========================================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartGreenhouse API v1",
        Version = "v1"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "SmartGreenhouse API v2",
        Version = "v2"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// =========================================================
// 5. BUILD APP
// =========================================================

var app = builder.Build();

// =========================================================
// 6. MIDDLEWARE PIPELINE
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var desc in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{desc.GroupName}/swagger.json",
            desc.GroupName.ToUpperInvariant());
    }
});

// =========================================================
// 7. ІНІЦІАЛІЗАЦІЯ БД
// =========================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Initialize(context);
}

// =========================================================
// 8. МАРШРУТИЗАЦІЯ
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.Run();
