using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Data;
using SmartGreenhouse.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGreenhouse.Web.Controllers.Api
{
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
        [ProducesResponseType(typeof(IEnumerable<Measurement>), 200)] // Вказуємо тип для Swagger
        public async Task<ActionResult<IEnumerable<Measurement>>> GetV1()
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
        [ProducesResponseType(typeof(MeasurementListDto), 200)] // Вказуємо тип для Swagger
        public async Task<ActionResult<MeasurementListDto>> GetV2()
        {
            var data = await _ctx.Measurements
                .Include(m => m.Sensor)
                .Include(m => m.User)
                .ToListAsync();

            // Використовуємо DTO
            var response = new MeasurementListDto
            {
                Count = data.Count,
                Items = data
            };

            return Ok(response);
        }

        // --- Методи Create, Update, Delete (залишаємо стандартними, додаємо версію) ---

        [HttpPost]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> Create(Measurement m)
        {
            _ctx.Measurements.Add(m);
            await _ctx.SaveChangesAsync();
            return Created($"api/v2/measurements/{m.Id}", m);
        }

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
}