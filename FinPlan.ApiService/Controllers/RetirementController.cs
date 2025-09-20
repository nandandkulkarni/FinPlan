using FinPlan.ApiService.Data;
using FinPlan.Shared.Models;
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
    public class RetirementController : ControllerBase
    {
        private readonly FinPlanDbContext _db;
        public RetirementController(FinPlanDbContext db)
        {
            _db = db;
        }

        // Simple ping for diagnostics
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try { Console.WriteLine("RetirementController.Ping called"); } catch { }
            return Ok("pong");
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

            PersistCalendarSpendingRequest? request = null;
            try
            {
                request = JsonSerializer.Deserialize<PersistCalendarSpendingRequest>(body, new JsonSerializerOptions
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

        [HttpGet("load")]
        public async Task<IActionResult> Load([FromQuery] string userGuid, [FromQuery] string calculatorType)
        {
            if (string.IsNullOrWhiteSpace(userGuid) || string.IsNullOrWhiteSpace(calculatorType))
                return BadRequest("Missing required fields.");

            var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == userGuid && x.CalculatorType == calculatorType);
            if (entity == null)
                return NotFound();

            // Return raw stored JSON string as application/json so callers can parse it directly
            try
            {
                return Content(entity.Data ?? string.Empty, "application/json");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error returning stored data: {ex.Message}");
                return Ok(entity.Data);
            }
        }

        //// Delete stored plan for a user/calculator
        //[HttpPost("delete")]
        //public async Task<IActionResult> Delete()
        //{
        //    string body;
        //    using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        //    {
        //        body = await reader.ReadToEndAsync();
        //    }

        //    if (string.IsNullOrWhiteSpace(body))
        //        return BadRequest("Empty request body.");

        //    // Try to deserialize to known request shapes to extract UserGuid and CalculatorType
        //    string? userGuid = null;
        //    string? calculatorType = null;

        //    try
        //    {
        //        var spReq = JsonSerializer.Deserialize<PersistSpendingRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //        if (spReq != null && !string.IsNullOrWhiteSpace(spReq.UserGuid) && !string.IsNullOrWhiteSpace(spReq.CalculatorType))
        //        {
        //            userGuid = spReq.UserGuid;
        //            calculatorType = spReq.CalculatorType;
        //        }
        //    }
        //    catch { /* ignore */ }

        //    if (userGuid == null || calculatorType == null)
        //    {
        //        try
        //        {
        //            var calReq = JsonSerializer.Deserialize<PersistCalendarSpendingRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //            if (calReq != null && !string.IsNullOrWhiteSpace(calReq.UserGuid) && !string.IsNullOrWhiteSpace(calReq.CalculatorType))
        //            {
        //                userGuid = calReq.UserGuid;
        //                calculatorType = calReq.CalculatorType;
        //            }
        //        }
        //        catch { /* ignore */ }
        //    }

        //    if (userGuid == null || calculatorType == null)
        //        return BadRequest("Request must contain UserGuid and CalculatorType.");

        //    var entity = await _db.FinPlans.FirstOrDefaultAsync(x => x.UserGuid == userGuid && x.CalculatorType == calculatorType);
        //    if (entity == null)
        //        return NotFound();

        //    _db.FinPlans.Remove(entity);
        //    await _db.SaveChangesAsync();
        //    return Ok();
        //}

    }
}
