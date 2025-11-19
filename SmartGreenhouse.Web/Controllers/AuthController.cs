using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Models;
using SmartGreenhouse.Web.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;


namespace SmartGreenhouse.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserStore _store;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserStore store, ILogger<AuthController> logger)
        {
            _store = store;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_store.UsernameExists(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Username already taken.");
                return View(model);
            }

            var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.Password));

            var user = new UserRecord(model.Username, model.FullName, model.Email, model.Phone, passwordHash, "User");
            _store.TryAdd(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim(ClaimTypes.Role, user.Role)
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

            var user = _store.GetByUsername(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            var providedHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.Password));
            if (user.PasswordHash != providedHash)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim(ClaimTypes.Role, user.Role)
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
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl ?? "/" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet]
        public IActionResult Profile()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");

            var model = new
            {
                Username = User.Identity.Name,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Phone = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value,
                Roles = string.Join(",", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal?.Claims.ToList() ?? new List<Claim>();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Привязка к локальному UserStore
            var user = _store.GetByEmail(email ?? "");
            if (user == null)
            {
                user = new UserRecord(email ?? name ?? "GoogleUser", name ?? "", email ?? "", "", "", "User");
                _store.TryAdd(user);
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Profile");
        }
    }
}
