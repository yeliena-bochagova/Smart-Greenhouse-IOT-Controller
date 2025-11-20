using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SmartGreenhouse.Web.Controllers
{
    public class Subroutine1Controller : Controller
    {
        // GET: Показати форму входу
        public IActionResult Index()
        {
            return View();
        }

        // POST: Обробити вхід
        [HttpPost]
        public IActionResult Login(string username)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                // Простий "фейковий" логін через сесію
                HttpContext.Session.SetString("User", username);
                return RedirectToAction("Index", "Subroutine2"); // Перехід до теплиці
            }
            return View("Index");
        }
    }
}