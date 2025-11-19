using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // новий метод для Welcome page
        public IActionResult Welcome()
        {
            return View();
        }

        // редирект на Welcome
        public IActionResult Index() => RedirectToAction("Welcome");

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
