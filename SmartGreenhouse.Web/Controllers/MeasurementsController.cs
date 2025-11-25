using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;

namespace SmartGreenhouse.Web.Controllers
{
    public class MeasurementsController : Controller
    {
        private readonly AppDbContext _context;

        public MeasurementsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var measurements = _context.Measurements
                .Include(m => m.Sensor)
                .ThenInclude(s => s.Plant)
                .OrderByDescending(m => m.Timestamp) 
                .Take(100) 
                .ToList();

            return View(measurements);
        }

        public IActionResult Details(int id)
        {
            var measurement = _context.Measurements
                .Include(m => m.Sensor)         
                .ThenInclude(s => s.Plant)       
                .FirstOrDefault(x => x.Id == id);

            if (measurement == null) return NotFound();
            
            return View(measurement);
        }
    }
}