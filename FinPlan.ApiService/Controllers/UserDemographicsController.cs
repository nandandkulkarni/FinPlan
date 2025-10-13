using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinPlan.ApiService.Data;
using FinPlan.Shared.Models.LivingCosts;

namespace FinPlan.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDemographicsController : MyControllerBase
{
    private readonly FinPlanDbContext _context;
    private readonly ILogger<UserDemographicsController> _logger;

    public UserDemographicsController(FinPlanDbContext context, ILogger<UserDemographicsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/userdemographics/{userGuid}
    [HttpGet("{userGuid}")]
    public async Task<ActionResult<UserDemographics>> GetUserDemographics(string userGuid)
    {
        try
        {
            var demographics = await _context.UserDemographics
                .FirstOrDefaultAsync(u => u.UserGuid == userGuid);

            if (demographics == null)
            {
                return NotFound($"Demographics for user '{userGuid}' not found");
            }

            return Ok(demographics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving demographics for user {UserGuid}", userGuid);
            return StatusCode(500, "An error occurred while retrieving user demographics");
        }
    }

    // POST: api/userdemographics
    [HttpPost]
    public async Task<ActionResult<UserDemographics>> CreateUserDemographics([FromBody] UserDemographics demographics)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(demographics.UserGuid))
            {
                return BadRequest("UserGuid is required");
            }

            var exists = await _context.UserDemographics.AnyAsync(u => u.UserGuid == demographics.UserGuid);
            if (exists)
            {
                return Conflict($"Demographics for user '{demographics.UserGuid}' already exist");
            }

            demographics.CreatedAt = GetEasternTime();
            demographics.UpdatedAt = GetEasternTime();

            _context.UserDemographics.Add(demographics);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserDemographics), new { userGuid = demographics.UserGuid }, demographics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user demographics");
            return StatusCode(500, "An error occurred while creating user demographics");
        }
    }

    // PUT: api/userdemographics/{userGuid}
    [HttpPut("{userGuid}")]
    public async Task<IActionResult> UpdateUserDemographics(string userGuid, [FromBody] UserDemographics demographics)
    {
        try
        {
            if (userGuid != demographics.UserGuid)
            {
                return BadRequest("UserGuid mismatch");
            }

            var existing = await _context.UserDemographics.FindAsync(userGuid);
            if (existing == null)
            {
                return NotFound($"Demographics for user '{userGuid}' not found");
            }

            existing.Age = demographics.Age;
            existing.MaritalStatus = demographics.MaritalStatus;
            existing.ChildrenAgesJSON = demographics.ChildrenAgesJSON;
            existing.PreferredCurrency = demographics.PreferredCurrency;
            existing.SelectedCityId = demographics.SelectedCityId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating demographics for user {UserGuid}", userGuid);
            return StatusCode(500, "An error occurred while updating user demographics");
        }
    }

    // DELETE: api/userdemographics/{userGuid}
    [HttpDelete("{userGuid}")]
    public async Task<IActionResult> DeleteUserDemographics(string userGuid)
    {
        try
        {
            var demographics = await _context.UserDemographics.FindAsync(userGuid);
            if (demographics == null)
            {
                return NotFound($"Demographics for user '{userGuid}' not found");
            }

            _context.UserDemographics.Remove(demographics);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting demographics for user {UserGuid}", userGuid);
            return StatusCode(500, "An error occurred while deleting user demographics");
        }
    }
}
