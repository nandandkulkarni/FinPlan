using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinPlan.ApiService.Data;

namespace FinPlan.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinPlanController : ControllerBase
    {
        private readonly FinPlanDbContext _db;
        public FinPlanController(FinPlanDbContext db)
        {
            _db = db;
        }

        // Save calculator data
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserGuid) || string.IsNullOrWhiteSpace(request.CalculatorType) || string.IsNullOrWhiteSpace(request.Data))
                return BadRequest("Missing required fields.");

            var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == request.UserGuid && x.CalculatorType == request.CalculatorType);
            if (entity == null)
            {
                entity = new FinPlanEntity
                {
                    Id = Guid.NewGuid(),
                    UserGuid = request.UserGuid,
                    CalculatorType = request.CalculatorType,
                    Data = request.Data
                };
                _db.FinPlans.Add(entity);
            }
            else
            {
                entity.Data = request.Data;
                _db.FinPlans.Update(entity);
            }
            await _db.SaveChangesAsync();
            return Ok();
        }

        // Load calculator data
        [HttpGet("load")]
        public async Task<IActionResult> Load([FromQuery] string userGuid, [FromQuery] string calculatorType)
        {
            if (string.IsNullOrWhiteSpace(userGuid) || string.IsNullOrWhiteSpace(calculatorType))
                return BadRequest("Missing required fields.");

            var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == userGuid && x.CalculatorType == calculatorType);
            if (entity == null)
                return NotFound();
            return Ok(entity.Data);
        }

        public class SaveRequest
        {
            public string UserGuid { get; set; } = string.Empty;
            public string CalculatorType { get; set; } = string.Empty;
            public string Data { get; set; } = string.Empty;
        }
    }
}
