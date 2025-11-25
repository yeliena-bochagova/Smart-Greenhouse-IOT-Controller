using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Services;
using Microsoft.AspNetCore.Authorization; 

namespace SmartGreenhouse.Web.Controllers
{
    [Authorize] // Пускаємо тільки своїх
    public class Subroutine3Controller : Controller
    {
        private readonly ISensorService _service;

        public Subroutine3Controller(ISensorService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            // Отримуємо ім'я користувача (якщо він авторизований)
            var username = User.Identity?.Name;

            // Отримуємо список логів (це List<string>), передаємо ім'я користувача
            var logs = _service.GetLogs(username);

            // Передаємо його у View
            return View(logs);
        }
    }
}
