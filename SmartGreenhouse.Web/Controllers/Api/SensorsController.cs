using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers.Api;

[ApiController]
[Route("api/v{version:apiVersion}/sensors")]
[ApiVersion("2.0")]
public class SensorsController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public SensorsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _ctx.Sensors.Include(s => s.Plant).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var s = await _ctx.Sensors.Include(s => s.Plant)
                                  .FirstOrDefaultAsync(x => x.Id == id);

        if (s == null) return NotFound();

        return Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Sensor s)
    {
        _ctx.Sensors.Add(s);
        await _ctx.SaveChangesAsync();
        return Created($"api/v2/sensors/{s.Id}", s);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Sensor updated)
    {
        var s = await _ctx.Sensors.FindAsync(id);
        if (s == null) return NotFound();

        s.Name = updated.Name;
        s.PlantId = updated.PlantId;

        await _ctx.SaveChangesAsync();
        return Ok(s);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _ctx.Sensors.FindAsync(id);
        if (s == null) return NotFound();

        _ctx.Sensors.Remove(s);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
