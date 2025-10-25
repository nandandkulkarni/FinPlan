using FinPlan.Web.Data;
using FinPlan.Web.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinPlan.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : MyControllerBase
    {
        private readonly FinPlanDbContext _db;
        public SurveyController(FinPlanDbContext db) => _db = db;

        [HttpPost("save")]
        public async Task<IActionResult> SaveSurvey([FromBody] SurveySaveRequest req)
        {
            var ipAddress = GetClientIpAddress();

            var existing = await _db.SurveyResponses
                .FirstOrDefaultAsync(x => x.UserGuid == req.UserGuid && x.SurveyType == req.SurveyType);

            if (existing == null)
            {
                existing = new SurveyResponse
                {
                    Id = Guid.NewGuid(),
                    UserGuid = req.UserGuid,
                    SurveyType = req.SurveyType,
                    SurveyJson = JsonSerializer.Serialize(req.SurveyJson),
                    CreatedAt = GetEasternTime(),
                    UpdatedAt = GetEasternTime(),
                    IpAddress = ipAddress
                };
                _db.SurveyResponses.Add(existing);
            }
            else
            {
                existing.SurveyJson = JsonSerializer.Serialize(req.SurveyJson);
                existing.UpdatedAt = GetEasternTime();
                existing.IpAddress = ipAddress;
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{surveyType}/{userGuid}")]
        public async Task<IActionResult> GetSurvey(string surveyType, string userGuid)
        {
            var existing = await _db.SurveyResponses
                .FirstOrDefaultAsync(x => x.UserGuid == userGuid && x.SurveyType == surveyType);

            if (existing == null)
                return NotFound();

            return Ok(existing.SurveyJson);
        }
    }
}
