using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SmartGreenhouse.Web.Controllers
{
    [Authorize]
    public class Subroutine2Controller : Controller
    {
        private readonly ISensorService _service;

        public Subroutine2Controller(ISensorService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            ViewBag.User = User.Identity.Name;
            var state = _service.GetState();
            return View(state);
        }

        // --- НОВИЙ МЕТОД ДЛЯ ФОНОВОГО ОНОВЛЕННЯ ---
        [HttpGet]
        public IActionResult GetSystemState()
        {
            // Повертаємо дані у форматі JSON, щоб JavaScript міг їх прочитати
            var state = _service.GetState();
            return Json(state);
        }
        // ------------------------------------------

        [HttpPost] public IActionResult ToggleHeater() { _service.ToggleHeater(); return RedirectToAction("Index"); }
        [HttpPost] public IActionResult ToggleVentilation() { _service.ToggleVentilation(); return RedirectToAction("Index"); }
        [HttpPost] public IActionResult Water() { _service.WaterPlants(); return RedirectToAction("Index"); }
        [HttpPost] public IActionResult AddLight() { _service.AddLight(); return RedirectToAction("Index"); }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(double lat, double lon, double volume)
        {
            await _service.UpdateCoordinatesAsync(lat, lon, volume);
            return RedirectToAction("Index");
        }
    }
}