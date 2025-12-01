using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Clients; 
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SmartGreenhouse.Web.Controllers
{
    public class V1Controller : Controller
    {
        private readonly ApiV1Client _v1Client;

        public V1Controller(ApiV1Client v1Client)
        {
            _v1Client = v1Client;
        }

        public async Task<IActionResult> Index()
        {
            // Перевірте ім'я методу! Швидше за все це GetV1Async або MeasurementsGETAsync
            // VS Code підкаже правильне ім'я після крапки.
            var measurements = await _v1Client.MeasurementsAsync();
            
            return View(measurements);
        }
    }
}