using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Models;
using SmartGreenhouse.Web.Data;
using System.Security.Claims;
using BCrypt.Net;


namespace SmartGreenhouse.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Username already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                // якщо хочеш мати ролі — додай поле Role у модель User
                // Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-login після реєстрації
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone)
                // new Claim(ClaimTypes.Role, user.Role) // тільки якщо є поле Role
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone)
                // new Claim(ClaimTypes.Role, user.Role) // тільки якщо є поле Role
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome", "Home");
        }

[HttpGet]
public IActionResult Profile()
{
    if (!User.Identity?.IsAuthenticated ?? false)
        return RedirectToAction("Login");

    var username = User.Identity?.Name;
    var user = _context.Users.FirstOrDefault(u => u.Username == username);
    if (user == null) return RedirectToAction("Login");

    var model = new UserProfileEditModel
    {
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Password = string.Empty,
        ConfirmPassword = string.Empty
    };

    return View(model);
}


        [HttpGet]
public IActionResult EditProfile()
{
    var username = User.Identity?.Name;
    var user = _context.Users.FirstOrDefault(u => u.Username == username);

    if (user == null) return RedirectToAction("Login");

    var model = new UserProfileEditModel
    {
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone
    };

    return View(model);
}

[HttpPost]
public async Task<IActionResult> EditProfile(UserProfileEditModel model)
{
    if (!ModelState.IsValid) return View(model);

        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login");
        }
    var user = _context.Users.FirstOrDefault(u => u.Username == username);
    if (user == null) return RedirectToAction("Login");

    // Перевірка унікальності Username
    if (_context.Users.Any(u => u.Username == model.Username && u.Id != user.Id))
    {
        ModelState.AddModelError(nameof(model.Username), "Username already taken.");
        return View(model);
    }

    user.Username = model.Username;
    user.FullName = model.FullName;
    user.Email = model.Email;
    user.Phone = model.Phone;

    // Оновлення паролю
    if (!string.IsNullOrEmpty(model.Password))
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
    }

    await _context.SaveChangesAsync();

    return RedirectToAction("Profile");
}
    }
}
