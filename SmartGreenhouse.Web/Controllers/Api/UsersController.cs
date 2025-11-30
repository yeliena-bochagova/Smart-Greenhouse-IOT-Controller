using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers.Api;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("2.0")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public UsersController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // GET: api/v2/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _ctx.Users.ToListAsync());
    }

    // GET: api/v2/users/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _ctx.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // PUT: api/v2/users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, User updated)
    {
        var user = await _ctx.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Username = updated.Username;
        user.Email = updated.Email;

        await _ctx.SaveChangesAsync();
        return Ok(user);
    }
}
