using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;

public class SensorsController : Controller
{
    private readonly AppDbContext _context;

    public SensorsController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var sensors = _context.Sensors.ToList();
        return View(sensors);
    }

    public IActionResult Details(int id)
    {
        var sensor = _context.Sensors.FirstOrDefault(s => s.Id == id);
        if (sensor == null) return NotFound();
        return View(sensor);
    }
}
