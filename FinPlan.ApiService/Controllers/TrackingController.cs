using FinPlan.ApiService.Data;
using Microsoft.AspNetCore.Mvc;

namespace FinPlan.ApiService.Controllers
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

            //// Determine IP address from forwarded headers, then remote as fallback
            //string? ip = null;
            //ip = First(Request.Headers["X-Forwarded-For"].FirstOrDefault());
            //if (string.IsNullOrWhiteSpace(ip)) ip = Request.Headers["X-Client-IP"].FirstOrDefault();
            //if (string.IsNullOrWhiteSpace(ip)) ip = Request.Headers["X-Real-IP"].FirstOrDefault();

            //if (string.IsNullOrWhiteSpace(ip))
            //{
            //    var rip = HttpContext.Connection.RemoteIpAddress;
            //    if (rip != null)
            //        ip = rip.IsIPv4MappedToIPv6 ? rip.MapToIPv4().ToString() : rip.ToString();
            //}

            var ip = GetClientIpAddress();

            // Prefer client-provided UA if supplied by the browser via Blazor
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
            public string Page { get; set; } = string.Empty; // logical page name
            public string? Route { get; set; }
            public string? UserGuid { get; set; }
            public string? UserAgent { get; set; }
        }
    }
}
