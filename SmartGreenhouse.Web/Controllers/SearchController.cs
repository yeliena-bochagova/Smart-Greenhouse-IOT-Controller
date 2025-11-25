using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartGreenhouse.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Search/Index
        public async Task<IActionResult> Index()
        {
            var sensors = await _context.Sensors.ToListAsync();
            return View(new SearchViewModel { Sensors = sensors });
        }

        // POST: Search/Results
        [HttpPost]
        public async Task<IActionResult> Results(SearchViewModel model)
        {
            // 1. Починаємо будувати запит
            var query = _context.Measurements
                .Include(m => m.Sensor)
                .ThenInclude(s => s.Plant)
                .AsQueryable();

            // 2. Застосовуємо фільтри
            if (model.SensorId.HasValue)
            {
                query = query.Where(m => m.SensorId == model.SensorId.Value);
            }

            if (model.FromDate.HasValue)
            {
                // Початок дня
                query = query.Where(m => m.Timestamp >= model.FromDate.Value);
            }

            if (model.ToDate.HasValue)
            {
                // Кінець дня (додаємо 23:59:59, щоб захопити весь день)
                var toDateEnd = model.ToDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.Timestamp <= toDateEnd);
            }

            if (model.MinValue.HasValue)
            {
                // Шукаємо по температурі (або можна змінити на Value)
                query = query.Where(m => m.Temperature >= model.MinValue.Value);
            }

            if (model.MaxValue.HasValue)
            {
                query = query.Where(m => m.Temperature <= model.MaxValue.Value);
            }

            // 3. Виконуємо запит (сортуємо: нові зверху, ліміт 500)
            var results = await query
                .OrderByDescending(m => m.Timestamp)
                .Take(500)
                .ToListAsync();

            return View(results);
        }
    }
}