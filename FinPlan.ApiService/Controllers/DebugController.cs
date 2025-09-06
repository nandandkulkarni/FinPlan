using FinPlan.ApiService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FinPlan.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly FinPlanDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DebugController(FinPlanDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Only enable in Development environment to avoid exposing internal data
        private bool IsAllowed() => _env.IsDevelopment();

        // List all FinPlan rows (lightweight metadata). Grouping will be done on client.
        [HttpGet("finplans")]
        public async Task<IActionResult> GetFinPlans()
        {
            if (!IsAllowed()) return NotFound();

            var list = await _db.FinPlans
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.UserGuid,
                    x.CalculatorType,
                    Length = (x.Data != null ? x.Data.Length : 0)
                })
                .ToListAsync();

            return Ok(list);
        }

        // Return the raw JSON string for a single FinPlan entry by id
        [HttpGet("finplans/{id:guid}")]
        public async Task<IActionResult> GetFinPlanById(Guid id)
        {
            if (!IsAllowed()) return NotFound();

            var entity = await _db.FinPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound();

            // Return raw stored JSON string
            return Ok(entity.Data);
        }
    }
}