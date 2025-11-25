using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;

namespace SmartGreenhouse.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // Стартовая страница поиска — форма
        public IActionResult Index()
        {
            var sensors = _context.Sensors.ToList();
            return View(new SearchViewModel { Sensors = sensors });
        }

        [HttpPost]
        public IActionResult Results(SearchViewModel model)
        {
            // JOIN Measurements → Sensors → Plants
            var query =
                from m in _context.Measurements
                join s in _context.Sensors on m.SensorId equals s.Id
                join p in _context.Plants on s.PlantId equals p.Id
                select new SearchResultViewModel
                {
                    Timestamp = m.Timestamp,
                    SensorName = s.Name,
                    PlantName = p.Name,
                    Value = m.Value
                };

            // Фильтры
            if (model.SensorId.HasValue)
            {
                var sensorName = _context.Sensors
                                         .First(s => s.Id == model.SensorId.Value).Name;
                query = query.Where(x => x.SensorName == sensorName);
            }

            if (model.FromDate.HasValue)
                query = query.Where(x => x.Timestamp >= model.FromDate.Value);

            if (model.ToDate.HasValue)
                query = query.Where(x => x.Timestamp <= model.ToDate.Value);

            if (model.MinValue.HasValue)
                query = query.Where(x => x.Value >= model.MinValue.Value);

            if (model.MaxValue.HasValue)
                query = query.Where(x => x.Value <= model.MaxValue.Value);

            return View(query.ToList());
        }
    }
}
