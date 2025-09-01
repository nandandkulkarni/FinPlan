using FinPlan.ApiService.Data;
using FinPlan.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> Save([FromBody] SaveSavingsRequest request)
        {
            var serializedData = System.Text.Json.JsonSerializer.Serialize(request.Data);

            var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == request.UserGuid && x.CalculatorType == request.CalculatorType);
            if (entity == null)
            {
                entity = new FinPlanEntity
                {
                    Id = Guid.NewGuid(),
                    UserGuid = request.UserGuid,
                    CalculatorType = request.CalculatorType,
                    Data = serializedData
                };
                _db.FinPlans.Add(entity);
            }
            else
            {
                entity.Data = serializedData;
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

            var loadedModel = System.Text.Json.JsonSerializer.Deserialize<SavingsCalculatorModel>(entity.Data, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return Ok(entity.Data);
        }

  

    }
}
