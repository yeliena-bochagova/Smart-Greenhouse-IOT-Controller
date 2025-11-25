using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartGreenhouse.Web.Controllers
{
    public class SensorsController : Controller
    {
        private readonly AppDbContext _context;

        public SensorsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var sensors = await _context.Sensors
                .Include(s => s.Plant) 
                .ToListAsync();
            return View(sensors);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sensor = await _context.Sensors
                .Include(s => s.Plant) 
                .Include(s => s.User)  
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sensor == null) return NotFound();

            return View(sensor);
        }
    }
}