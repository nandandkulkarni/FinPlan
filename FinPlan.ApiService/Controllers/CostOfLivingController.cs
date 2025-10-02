using FinPlan.ApiService.Data;
using FinPlan.Shared.Models.LivingCosts;
using FinPlan.Shared.Models.Savings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FinPlan.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostOfLivingController : ControllerBase
    {
        private readonly FinPlanDbContext _db;
        public CostOfLivingController(FinPlanDbContext db)
        {
            _db = db;
        }

        // Helper method to get the client IP address

        private string GetClientIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        private string GetClientIpAddress1()
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

            PersistCostOfLivingRequest? request = null;
            try
            {
                request = JsonSerializer.Deserialize<PersistCostOfLivingRequest>(body, new JsonSerializerOptions
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
                return BadRequest("Request could not be deserialized to PersistCostOfLivingRequest.");

            // Get client IP address
            var clientIpAddress = GetClientIpAddress();

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.FinPlans.Add(entity);
            }
            else
            {
                entity.Data = serializedData;
                entity.IpAddress = clientIpAddress;
                entity.UpdatedAt = DateTime.UtcNow;
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

            // Return the stored JSON payload directly
            return Ok(entity.Data);
        }

        // Return list of saved calculatorType keys for the user that start with CostOfLiving-
        [HttpGet("tabs")]
        public async Task<IActionResult> Tabs([FromQuery] string userGuid)
        {
            if (string.IsNullOrWhiteSpace(userGuid))
                return BadRequest("Missing userGuid.");

            var list = await _db.FinPlans
                .Where(x => x.UserGuid == userGuid && x.CalculatorType.StartsWith("CostOfLiving-"))
                .Select(x => x.CalculatorType)
                .ToListAsync();

            return Ok(list);
        }

        // Delete a saved tab (calculator entry)
        [HttpDelete("tabs")]
        public async Task<IActionResult> DeleteTab([FromQuery] string userGuid, [FromQuery] string calculatorType)
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
