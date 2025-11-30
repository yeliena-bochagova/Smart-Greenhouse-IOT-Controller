using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers.Api;

[ApiController]
[Route("api/v{version:apiVersion}/measurements")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class MeasurementsController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public MeasurementsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // ----------------- V1 -----------------
    /// <summary>
    /// Отримати всі вимірювання (v1)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetV1()
    {
        var data = await _ctx.Measurements.ToListAsync();
        return Ok(data);
    }

    // ----------------- V2 -----------------
    /// <summary>
    /// Отримати всі вимірювання з деталями (v2)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetV2()
    {
        var data = await _ctx.Measurements
            .Include(m => m.Sensor)
            .Include(m => m.User)
            .ToListAsync();

        return Ok(new {
            Count = data.Count,
            Items = data
        });
    }

    /// <summary>
    /// Отримати вимірювання за Id (v2)
    /// </summary>
    [HttpGet("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _ctx.Measurements
            .Include(x => x.Sensor)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m == null) return NotFound();
        return Ok(m);
    }

    /// <summary>
    /// Створити нове вимірювання (v2)
    /// </summary>
    [HttpPost]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> Create(Measurement m)
    {
        _ctx.Measurements.Add(m);
        await _ctx.SaveChangesAsync();
        return Created($"api/v2/measurements/{m.Id}", m);
    }

    /// <summary>
    /// Оновити вимірювання за Id (v2)
    /// </summary>
    [HttpPut("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> Update(int id, Measurement updated)
    {
        var m = await _ctx.Measurements.FindAsync(id);
        if (m == null) return NotFound();

        m.SensorId = updated.SensorId;
        m.UserId = updated.UserId;
        m.Temperature = updated.Temperature;
        m.Humidity = updated.Humidity;
        m.Timestamp = updated.Timestamp;

        await _ctx.SaveChangesAsync();
        return Ok(m);
    }

    /// <summary>
    /// Видалити вимірювання за Id (v2)
    /// </summary>
    [HttpDelete("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _ctx.Measurements.FindAsync(id);
        if (m == null) return NotFound();

        _ctx.Measurements.Remove(m);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
