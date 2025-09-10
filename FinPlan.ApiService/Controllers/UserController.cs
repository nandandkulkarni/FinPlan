using FinPlan.ApiService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinPlan.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
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
                Console.WriteLine($"UpsertUser called for UserGuid={request.UserGuid}; Email={request.Email}; First={request.FirstName}; Last={request.LastName}");

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
                        CreatedAt = DateTime.UtcNow,
                        LastSignInAt = DateTime.UtcNow
                    };
                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();

                    // Return created user info
                    return Ok(new {
                        user.Id,
                        user.UserGuid,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.DisplayName,
                        user.Provider,
                        user.CreatedAt,
                        user.LastSignInAt
                    });
                }
                else
                {
                    existing.Email = request.Email ?? existing.Email;
                    existing.FirstName = request.FirstName ?? existing.FirstName;
                    existing.LastName = request.LastName ?? existing.LastName;
                    existing.DisplayName = request.DisplayName ?? existing.DisplayName;
                    existing.Provider = request.Provider ?? existing.Provider;
                    existing.LastSignInAt = DateTime.UtcNow;
                    _db.Users.Update(existing);
                    await _db.SaveChangesAsync();

                    return Ok(new {
                        existing.Id,
                        existing.UserGuid,
                        existing.Email,
                        existing.FirstName,
                        existing.LastName,
                        existing.DisplayName,
                        existing.Provider,
                        existing.CreatedAt,
                        existing.LastSignInAt
                    });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"UpsertUser exception: {ex.Message}");
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
        }
    }
}
