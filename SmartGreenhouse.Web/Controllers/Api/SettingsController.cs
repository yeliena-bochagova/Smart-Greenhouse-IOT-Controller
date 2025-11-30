using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers.Api;

[ApiController]
[Route("api/v{version:apiVersion}/settings")]
[ApiVersion("2.0")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public SettingsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // GET: api/v2/settings
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _ctx.Settings.FirstOrDefaultAsync();
        if (settings == null) return NotFound();
        return Ok(settings);
    }

    // PUT: api/v2/settings
    [HttpPut]
    public async Task<IActionResult> Update(GreenhouseSettings updated)
    {
        var settings = await _ctx.Settings.FirstOrDefaultAsync();
        if (settings == null) return NotFound();

        settings.MinTemperature = updated.MinTemperature;
        settings.MaxTemperature = updated.MaxTemperature;
        settings.MinHumidity = updated.MinHumidity;
        settings.MaxHumidity = updated.MaxHumidity;

        await _ctx.SaveChangesAsync();
        return Ok(settings);
    }
}
