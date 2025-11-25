using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Linq;

public class MeasurementsController : Controller
{
    private readonly AppDbContext _context;

    public MeasurementsController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var measurements = _context.Measurements.ToList();
        return View(measurements);
    }

    public IActionResult Details(int id)
    {
        var measurement = _context.Measurements.FirstOrDefault(x => x.Id == id);
        if (measurement == null) return NotFound();
        return View(measurement);
    }
}
