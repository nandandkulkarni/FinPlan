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

        private string GetClientIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        // Helper method to get the client IP address
        private string GetClientIpAddress1()
        {
            string? ipAddress = null;

            // Check for forwarded IP addresses (when behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }
            }

            // Check for X-Real-IP header
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            // Check CF-Connecting-IP (Cloudflare)
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                ipAddress = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            }

            // Check X-Forwarded-Proto and other common headers
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("X-Client-IP"))
            {
                ipAddress = Request.Headers["X-Client-IP"].FirstOrDefault();
            }

            // Fall back to remote IP address
            if (string.IsNullOrEmpty(ipAddress))
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                if (remoteIp != null)
                {
                    // Handle IPv6 loopback (::1) and IPv4 loopback (127.0.0.1)
                    if (remoteIp.IsIPv4MappedToIPv6)
                    {
                        ipAddress = remoteIp.MapToIPv4().ToString();
                    }
                    else
                    {
                        ipAddress = remoteIp.ToString();
                    }

                    // Replace localhost addresses with a more meaningful value
                    if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                    {
                        ipAddress = "localhost";
                    }
                }
            }

            // Validate the IP address format
            if (!string.IsNullOrEmpty(ipAddress) &&
                !ipAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                System.Net.IPAddress.TryParse(ipAddress, out var validIp))
            {
                // Additional filtering for private/local addresses if needed
                if (IsPrivateOrLocalAddress(validIp))
                {
                    return $"private-{ipAddress}";
                }
                return ipAddress;
            }

            return "Unknown";
        }

        private bool IsPrivateOrLocalAddress(System.Net.IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = ipAddress.GetAddressBytes();

                // Check for private IPv4 ranges
                // 10.0.0.0/8
                if (bytes[0] == 10) return true;

                // 172.16.0.0/12
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168) return true;

                // 127.0.0.0/8 (loopback)
                if (bytes[0] == 127) return true;
            }
            else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                // Check for IPv6 loopback and private addresses
                if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal ||
                    System.Net.IPAddress.IsLoopback(ipAddress))
                {
                    return true;
                }
            }

            return false;
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
