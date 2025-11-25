using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;

namespace SmartGreenhouse.Web.Controllers
{
    public class PlantsController : Controller
    {
        private readonly AppDbContext _context;

        public PlantsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var plants = _context.Plants.ToList();
            return View(plants);
        }

        public IActionResult Details(int id)
        {
            var plant = _context.Plants
                .Include(p => p.Sensors) 
                .FirstOrDefault(p => p.Id == id);

            if (plant == null) return NotFound();
            
            return View(plant);
        }
    }
}