using FinPlan.ApiService.Data;
using FinPlan.Shared.Models.Savings;
using FinPlan.Shared.Models.Spending;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

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

        // Helper method to get the client IP address
        private string GetClientIpAddress()
        {
            string? ipAddress = null;

            // Check for forwarded IP addresses (when behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    ipAddress = ipAddress.Split(',')[0].Trim();
                }
            }

            // Check for X-Real-IP header
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            // Fall back to remote IP address
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress ?? "Unknown";
        }

        // Helper method to get current Eastern Time
        private DateTime GetEasternTime()
        {
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);
        }

        // Save calculator data - read raw body and deserialize to return clearer errors when JSON is invalid
        [HttpPost("save")]
        public async Task<IActionResult> Save()
        {
            // Read the raw request body so we can return a helpful error if deserialization fails
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body))
                return BadRequest("Empty request body.");

            PersistSavingsRequest? request = null;
            try
            {
                request = JsonSerializer.Deserialize<PersistSavingsRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException jex)
            {
                // Invalid JSON - return details to the caller (useful for dev)
                return BadRequest($"Invalid JSON in request body: {jex.Message}");
            }

            if (request == null)
                return BadRequest("Request could not be deserialized to PersistCalendarSpendingRequest.");

            // Get client IP address and Eastern Time
            var clientIpAddress = GetClientIpAddress();
            var easternTime = GetEasternTime();

            // Proceed as before
            var serializedData = System.Text.Json.JsonSerializer.Serialize(request.Data);

            var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == request.UserGuid && x.CalculatorType == request.CalculatorType);
            if (entity == null)
            {
                entity = new FinPlanEntity
                {
                    Id = Guid.NewGuid(),
                    UserGuid = request.UserGuid,
                    CalculatorType = request.CalculatorType,
                    Data = serializedData,
                    IpAddress = clientIpAddress,
                    CreatedAt = easternTime,
                    UpdatedAt = easternTime
                };
                _db.FinPlans.Add(entity);
            }
            else
            {
                entity.Data = serializedData;
                entity.IpAddress = clientIpAddress;
                entity.UpdatedAt = easternTime;
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

        // Delete saved calculator data
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] string userGuid, [FromQuery] string calculatorType)
        {
            if (string.IsNullOrWhiteSpace(userGuid) || string.IsNullOrWhiteSpace(calculatorType))
                return BadRequest("Missing required fields.");

            var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == userGuid && x.CalculatorType == calculatorType);
            if (entity == null)
                return NotFound();

            _db.FinPlans.Remove(entity);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
