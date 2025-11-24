using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Security.Claims;

[Route("[controller]/[action]")]
public class ExternalAuthController : Controller
{
    private readonly AppDbContext _context;

    public ExternalAuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GoogleLogin(string returnUrl = "/")
    {
        var redirectUrl = Url.Action("GoogleResponse", "ExternalAuth", new { ReturnUrl = returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleResponse(string returnUrl = "/Auth/Profile")
    {
        // Отримуємо результат автентифікації від Google
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded) return RedirectToAction("Login", "Auth");

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        // Шукаємо користувача у БД по email
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Username = email ?? name ?? "google_user",
                FullName = name ?? string.Empty,
                Email = email ?? string.Empty,
                Phone = string.Empty, // Google не повертає телефон
                PasswordHash = string.Empty // для Google‑користувачів пароль не потрібен
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Створюємо кукі для локальної сесії
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FullName ?? string.Empty),
            new Claim(ClaimTypes.MobilePhone, user.Phone ?? string.Empty)
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToAction("Profile", "Auth");
    }
}
