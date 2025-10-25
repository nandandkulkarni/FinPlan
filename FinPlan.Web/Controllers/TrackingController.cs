using FinPlan.Web.Data;
using FinPlan.Web.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinPlan.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackingController : MyControllerBase
    {
        private readonly FinPlanDbContext _db;
        public TrackingController(FinPlanDbContext db)
        {
            _db = db;
        }

        private static string First(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return string.Empty;
            var parts = csv.Split(',');
            return parts.Length > 0 ? parts[0].Trim() : csv;
        }

        [HttpPost("pageview")]
        public async Task<IActionResult> TrackPageView([FromBody] PageViewDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Page))
                return BadRequest("Missing page");

            var ip = GetClientIpAddress();

            var userAgent = !string.IsNullOrWhiteSpace(dto.UserAgent)
                ? dto.UserAgent
                : Request.Headers["User-Agent"].FirstOrDefault();

            var referrer = Request.Headers["Referrer"].FirstOrDefault();

            var pv = new PageView
            {
                Id = Guid.NewGuid(),
                Page = dto.Page,
                Route = dto.Route,
                UserGuid = dto.UserGuid,
                IpAddress = ip,
                UserAgent = userAgent,
                Referrer = referrer,
                CreatedAt = GetEasternTime()
            };

            _db.PageViews.Add(pv);
            await _db.SaveChangesAsync();
            return Ok();
        }

        public class PageViewDto
        {
            public string Page { get; set; } = string.Empty;
            public string? Route { get; set; }
            public string? UserGuid { get; set; }
            public string? UserAgent { get; set; }
        }
    }
}
