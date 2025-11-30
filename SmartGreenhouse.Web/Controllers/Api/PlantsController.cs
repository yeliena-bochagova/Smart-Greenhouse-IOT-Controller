using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers.Api;

[ApiController]
[Route("api/v{version:apiVersion}/plants")]
[ApiVersion("2.0")]
public class PlantsController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public PlantsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // GET: api/v2/plants
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _ctx.Plants.ToListAsync());
    }

    // GET: api/v2/plants/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var plant = await _ctx.Plants.FindAsync(id);
        if (plant == null) return NotFound();
        return Ok(plant);
    }

    // POST: api/v2/plants
    [HttpPost]
    public async Task<IActionResult> Create(Plant plant)
    {
        _ctx.Plants.Add(plant);
        await _ctx.SaveChangesAsync();
        return Created($"api/v2/plants/{plant.Id}", plant);
    }

    // PUT: api/v2/plants/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Plant updated)
    {
        var plant = await _ctx.Plants.FindAsync(id);
        if (plant == null) return NotFound();

        plant.Name = updated.Name;
        plant.Description = updated.Description;

        await _ctx.SaveChangesAsync();
        return Ok(plant);
    }

    // DELETE: api/v2/plants/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var plant = await _ctx.Plants.FindAsync(id);
        if (plant == null) return NotFound();

        _ctx.Plants.Remove(plant);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}
