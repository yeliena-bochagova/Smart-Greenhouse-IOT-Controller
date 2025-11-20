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
            // Отримуємо список логів (це List<string>)
            var logs = _service.GetLogs();
            
            // Передаємо його у View
            return View(logs);
        }
    }
}