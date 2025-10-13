using FinPlan.ApiService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinPlan.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : MyControllerBase
    {
        private readonly FinPlanDbContext _db;
        public UserController(FinPlanDbContext db)
        {
            _db = db;
        }

        // Upsert user record. Accepts a minimal payload.
        [HttpPost("upsert")]
        public async Task<IActionResult> UpsertUser([FromBody] UpsertUserRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserGuid))
            {
                Console.Error.WriteLine("UpsertUser called with invalid request");
                return BadRequest("Invalid request");
            }

            try
            {
                Console.WriteLine($"UpsertUser called for UserGuid={request.UserGuid}; Email={request.Email}; First={request.FirstName}; Last={request.LastName}; CookieGuid={request.CookieGuid}");

                // If client supplied a cookie-guid, check whether that cookie-guid already has FinPlan rows
                // and there is no registration for this email. If so, require user confirmation before associating.
                if (!string.IsNullOrWhiteSpace(request.CookieGuid) && !string.IsNullOrWhiteSpace(request.Email))
                {
                    var cookieGuid = request.CookieGuid;
                    var hasFinplans = await _db.FinPlans.AnyAsync(f => f.UserGuid == cookieGuid);
                    var hasRegistration = await _db.UserRegistrations.AnyAsync(r => r.UserEmail == request.Email);
                    if (hasFinplans && !hasRegistration)
                    {
                        // return a response indicating association is required
                        var count = await _db.FinPlans.CountAsync(f => f.UserGuid == cookieGuid);
                        return Ok(new { needsCookieAssociation = true, cookieGuid = cookieGuid, itemCount = count });
                    }
                }

                var existing = await _db.Users.FirstOrDefaultAsync(u => u.UserGuid == request.UserGuid);
                if (existing == null)
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        UserGuid = request.UserGuid,
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        DisplayName = request.DisplayName,
                        Provider = request.Provider,
                        CreatedAt = GetEasternTime(),
                        LastSignInAt = GetEasternTime()
                    };
                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();

                    // Do NOT auto-create UserRegistration here - association must be explicit

                    // Return created user info
                    return Ok(new { needsCookieAssociation = false, user = new {
                        user.Id,
                        user.UserGuid,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.DisplayName,
                        user.Provider,
                        user.CreatedAt,
                        user.LastSignInAt
                    }});
                }
                else
                {
                    existing.Email = request.Email ?? existing.Email;
                    existing.FirstName = request.FirstName ?? existing.FirstName;
                    existing.LastName = request.LastName ?? existing.LastName;
                    existing.DisplayName = request.DisplayName ?? existing.DisplayName;
                    existing.Provider = request.Provider ?? existing.Provider;
                    existing.LastSignInAt = GetEasternTime();
                    _db.Users.Update(existing);
                    await _db.SaveChangesAsync();

                    // Do NOT auto-create UserRegistration here - association must be explicit

                    return Ok(new { needsCookieAssociation = false, user = new {
                        existing.Id,
                        existing.UserGuid,
                        existing.Email,
                        existing.FirstName,
                        existing.LastName,
                        existing.DisplayName,
                        existing.Provider,
                        existing.CreatedAt,
                        existing.LastSignInAt
                    }});
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"UpsertUser exception: {ex.Message}");
                return StatusCode(500, "Server error");
            }
        }

        [HttpPost("associate-cookie")]
        public async Task<IActionResult> AssociateCookie([FromBody] AssociateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserEmail) || string.IsNullOrWhiteSpace(request.CookieGuid))
                return BadRequest("Missing fields");

            try
            {
                // If registration already exists, return ok
                var existing = await _db.UserRegistrations.FirstOrDefaultAsync(r => r.UserEmail == request.UserEmail);
                if (existing != null)
                    return Ok(new { associated = false, reason = "already_exists" });

                var reg = new UserRegistration
                {
                    Id = Guid.NewGuid(),
                    UserEmail = request.UserEmail,
                    CookieGuid = request.CookieGuid,
                    CreatedDate = GetEasternTime()
                };
                _db.UserRegistrations.Add(reg);
                await _db.SaveChangesAsync();
                Console.WriteLine($"Associated cookie {request.CookieGuid} to email {request.UserEmail}");
                return Ok(new { associated = true });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"AssociateCookie exception: {ex.Message}");
                return StatusCode(500, "Server error");
            }
        }

        // Simple request DTO
        public class UpsertUserRequest
        {
            public string UserGuid { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? DisplayName { get; set; }
            public string? Provider { get; set; }

            // optional cookie-guid to correlate client-side cookie to registration
            public string? CookieGuid { get; set; }
        }

        public class AssociateRequest
        {
            public string? UserEmail { get; set; }
            public string? CookieGuid { get; set; }
        }
    }
}
