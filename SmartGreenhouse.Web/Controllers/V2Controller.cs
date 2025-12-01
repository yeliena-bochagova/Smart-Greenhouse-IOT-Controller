using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Clients; 
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace SmartGreenhouse.Web.Controllers
{
    public class V2Controller : Controller
    {
        private readonly ApiV2Client _v2Client;

        public V2Controller(ApiV2Client v2Client)
        {
            _v2Client = v2Client;
        }

        public async Task<IActionResult> Index()
        {
            // Перевірте ім'я методу! Швидше за все це GetV2Async
            var result = await _v2Client.MeasurementsGETAsync();

            // Тепер result - це MeasurementListDto, у нього є поле Items
            var measurements = result.Items ?? new List<Measurement>();

            if (measurements.Any())
            {
                ViewBag.TotalRecords = result.Count; // Беремо з DTO
                ViewBag.AvgTemperature = measurements.Average(m => m.Temperature); 
                ViewBag.LatestMeasurement = measurements.OrderByDescending(m => m.Timestamp).FirstOrDefault();
            }
            else
            {
                ViewBag.TotalRecords = 0;
                ViewBag.AvgTemperature = 0.0;
            }

            return View(measurements); 
        }
    }
}