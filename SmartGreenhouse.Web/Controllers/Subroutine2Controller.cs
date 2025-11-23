using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using SmartGreenhouse.Web.Services;
using System.Threading.Tasks;
using System.Linq;

namespace SmartGreenhouse.Web.Controllers
{
    [Authorize]
    public class Subroutine2Controller : Controller
    {
        private readonly ISensorService _service;
        private readonly AppDbContext _context;

        public Subroutine2Controller(ISensorService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        public IActionResult Index()
        {
            var username = User.Identity?.Name;
            ViewBag.User = username;

            var state = _service.GetState(username);

            var user = _context.Users.Include(u => u.Settings)
                                     .FirstOrDefault(u => u.Username == username);
            if (user?.Settings != null)
            {
                ViewBag.Settings = user.Settings;
            }

            return View(state);
        }

        // --- Фонове оновлення стану ---
        [HttpGet]
        public IActionResult GetSystemState()
        {
            var username = User.Identity?.Name;
            var state = _service.GetState(username);
            return Json(state);
        }

        [HttpPost]
        public IActionResult ToggleHeater()
        {
            var username = User.Identity?.Name;
            _service.ToggleHeater(username);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleVentilation()
        {
            var username = User.Identity?.Name;
            _service.ToggleVentilation(username);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Water()
        {
            var username = User.Identity?.Name;
            _service.WaterPlants(username);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddLight()
        {
            var username = User.Identity?.Name;
            _service.AddLight(username);
            return RedirectToAction("Index");
        }

        // --- Збереження налаштувань у БД ---
        [HttpPost]
        public async Task<IActionResult> UpdateSettings(double minTemp, double maxTemp, double minHum, double maxHum)
        {
            var username = User.Identity?.Name;
            var user = _context.Users.Include(u => u.Settings)
                                     .FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                if (user.Settings == null)
                    user.Settings = new GreenhouseSettings();

                user.Settings.MinTemperature = minTemp;
                user.Settings.MaxTemperature = maxTemp;
                user.Settings.MinHumidity = minHum;
                user.Settings.MaxHumidity = maxHum;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // --- Оновлення координат теплиці ---
        [HttpPost]
        public async Task<IActionResult> UpdateCoordinates(double lat, double lon, double volume)
        {
            var username = User.Identity?.Name;
            await _service.UpdateCoordinatesAsync(lat, lon, volume, username);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAll(
            double minTemp, double maxTemp, double minHum, double maxHum,
            double lat, double lon, double volume)
        {
            var username = User.Identity?.Name;

            // 1. Оновлюємо налаштування в БД
            var user = _context.Users.Include(u => u.Settings)
                                    .FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                if (user.Settings == null)
                    user.Settings = new GreenhouseSettings();

                user.Settings.MinTemperature = minTemp;
                user.Settings.MaxTemperature = maxTemp;
                user.Settings.MinHumidity = minHum;
                user.Settings.MaxHumidity = maxHum;

                await _context.SaveChangesAsync();
            }

            // 2. Оновлюємо координати у SensorService (пам’ять)
            await _service.UpdateCoordinatesAsync(lat, lon, volume, username);

            // 3. Повертаємося на Index, щоб відобразити нові дані
            return RedirectToAction("Index");
        }

    }
}
