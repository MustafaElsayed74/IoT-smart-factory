using Microsoft.AspNetCore.Mvc;
using IoTBackend.Data;
using IoTBackend.Models;

namespace IoTBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ReadingsController> _logger;

        public ReadingsController(AppDbContext dbContext, ILogger<ReadingsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Reading>> GetReadings()
        {
            var readings = _dbContext.Readings.OrderByDescending(r => r.Timestamp).Take(100).ToList();
            return Ok(readings);
        }

        [HttpGet("{id}")]
        public ActionResult<Reading> GetReading(int id)
        {
            var reading = _dbContext.Readings.FirstOrDefault(r => r.ReadingId == id);
            if (reading == null)
                return NotFound();
            return Ok(reading);
        }

        [HttpPost]
        public async Task<ActionResult<Reading>> CreateReading([FromBody] Reading reading)
        {
            reading.Timestamp = DateTime.UtcNow;
            _dbContext.Readings.Add(reading);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReading), new { id = reading.ReadingId }, reading);
        }
    }
}
