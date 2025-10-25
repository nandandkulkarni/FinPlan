using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinPlan.Web.Data;
using FinPlan.Shared.Models.LivingCosts;

namespace FinPlan.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CityTemplateController : MyControllerBase
    {
        private readonly FinPlanDbContext _context;
        private readonly ILogger<CityTemplateController> _logger;

        public CityTemplateController(FinPlanDbContext context, ILogger<CityTemplateController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityTemplate>>> GetAllCities()
        {
            try
            {
                var cities = await _context.CityTemplates
                    .OrderBy(c => c.Country)
                    .ThenBy(c => c.CityName)
                    .ToListAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city templates");
                return StatusCode(500, "An error occurred while retrieving city templates");
            }
        }

        [HttpGet("{cityId}")]
        public async Task<ActionResult<CityTemplate>> GetCity(string cityId)
        {
            try
            {
                var city = await _context.CityTemplates
                    .FirstOrDefaultAsync(c => c.CityId == cityId);

                if (city == null)
                {
                    return NotFound($"City with ID '{cityId}' not found");
                }

                return Ok(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving city template {CityId}", cityId);
                return StatusCode(500, "An error occurred while retrieving the city template");
            }
        }

        [HttpGet("{cityId}/profiles")]
        public async Task<ActionResult<IEnumerable<DemographicProfile>>> GetCityProfiles(string cityId)
        {
            try
            {
                var cityExists = await _context.CityTemplates.AnyAsync(c => c.CityId == cityId);
                if (!cityExists)
                {
                    return NotFound($"City with ID '{cityId}' not found");
                }

                var profiles = await _context.DemographicProfiles
                    .Where(p => p.CityId == cityId)
                    .OrderBy(p => p.AgeMin)
                    .ThenBy(p => p.MaritalStatus)
                    .ToListAsync();

                foreach (var profile in profiles)
                {
                    profile.DeserializeFromDatabase();
                }

                return Ok(profiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profiles for city {CityId}", cityId);
                return StatusCode(500, "An error occurred while retrieving city profiles");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CityTemplate>> CreateCity([FromBody] CityTemplate city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city.CityId))
                {
                    return BadRequest("CityId is required");
                }

                var exists = await _context.CityTemplates.AnyAsync(c => c.CityId == city.CityId);
                if (exists)
                {
                    return Conflict($"City with ID '{city.CityId}' already exists");
                }

                city.CreatedAt = GetEasternTime();
                city.UpdatedAt = GetEasternTime();

                _context.CityTemplates.Add(city);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCity), new { cityId = city.CityId }, city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city template");
                return StatusCode(500, "An error occurred while creating the city template");
            }
        }

        [HttpPut("{cityId}")]
        public async Task<IActionResult> UpdateCity(string cityId, [FromBody] CityTemplate city)
        {
            try
            {
                if (cityId != city.CityId)
                {
                    return BadRequest("City ID mismatch");
                }

                var existingCity = await _context.CityTemplates.FindAsync(cityId);
                if (existingCity == null)
                {
                    return NotFound($"City with ID '{cityId}' not found");
                }

                existingCity.CityName = city.CityName;
                existingCity.Country = city.Country;
                existingCity.Currency = city.Currency;
                existingCity.CostOfLivingIndex = city.CostOfLivingIndex;
                existingCity.CreatedBy = city.CreatedBy;
                existingCity.UpdatedAt = GetEasternTime();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city template {CityId}", cityId);
                return StatusCode(500, "An error occurred while updating the city template");
            }
        }

        [HttpDelete("{cityId}")]
        public async Task<IActionResult> DeleteCity(string cityId)
        {
            try
            {
                var city = await _context.CityTemplates.FindAsync(cityId);
                if (city == null)
                {
                    return NotFound($"City with ID '{cityId}' not found");
                }

                _context.CityTemplates.Remove(city);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city template {CityId}", cityId);
                return StatusCode(500, "An error occurred while deleting the city template");
            }
        }

        [HttpPost("{cityId}/profiles")]
        public async Task<ActionResult<DemographicProfile>> CreateProfile(string cityId, [FromBody] DemographicProfile profile)
        {
            try
            {
                var cityExists = await _context.CityTemplates.AnyAsync(c => c.CityId == cityId);
                if (!cityExists)
                {
                    return NotFound($"City with ID '{cityId}' not found");
                }

                if (string.IsNullOrWhiteSpace(profile.ProfileId))
                {
                    return BadRequest("ProfileId is required");
                }

                var exists = await _context.DemographicProfiles.AnyAsync(p => p.ProfileId == profile.ProfileId);
                if (exists)
                {
                    return Conflict($"Profile with ID '{profile.ProfileId}' already exists");
                }

                profile.CityId = cityId;
                profile.CreatedAt = GetEasternTime();
                profile.UpdatedAt = GetEasternTime();

                _context.DemographicProfiles.Add(profile);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProfile), new { profileId = profile.ProfileId }, profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating demographic profile for city {CityId}", cityId);
                return StatusCode(500, "An error occurred while creating the demographic profile");
            }
        }

        [HttpGet("profile/{profileId}")]
        public async Task<ActionResult<DemographicProfile>> GetProfile(string profileId)
        {
            try
            {
                var profile = await _context.DemographicProfiles
                    .FirstOrDefaultAsync(p => p.ProfileId == profileId);

                if (profile == null)
                {
                    return NotFound($"Profile with ID '{profileId}' not found");
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile {ProfileId}", profileId);
                return StatusCode(500, "An error occurred while retrieving the profile");
            }
        }

        [HttpPut("profile/{profileId}")]
        public async Task<IActionResult> UpdateProfile(string profileId, [FromBody] DemographicProfile profile)
        {
            try
            {
                if (profileId != profile.ProfileId)
                {
                    return BadRequest("Profile ID mismatch");
                }

                var existingProfile = await _context.DemographicProfiles.FindAsync(profileId);
                if (existingProfile == null)
                {
                    return NotFound($"Profile with ID '{profileId}' not found");
                }

                existingProfile.ProfileName = profile.ProfileName;
                existingProfile.AgeMin = profile.AgeMin;
                existingProfile.AgeMax = profile.AgeMax;
                existingProfile.MaritalStatus = profile.MaritalStatus;
                existingProfile.ChildrenCount = profile.ChildrenCount;
                existingProfile.ChildrenAgesJSON = profile.ChildrenAgesJSON;
                existingProfile.SampleExpensesJSON = profile.SampleExpensesJSON;
                existingProfile.UpdatedAt = GetEasternTime();

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile {ProfileId}", profileId);
                return StatusCode(500, "An error occurred while updating the profile");
            }
        }

        [HttpDelete("profile/{profileId}")]
        public async Task<IActionResult> DeleteProfile(string profileId)
        {
            try
            {
                var profile = await _context.DemographicProfiles.FindAsync(profileId);
                if (profile == null)
                {
                    return NotFound($"Profile with ID '{profileId}' not found");
                }

                _context.DemographicProfiles.Remove(profile);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile {ProfileId}", profileId);
                return StatusCode(500, "An error occurred while deleting the profile");
            }
        }

        [HttpGet("match")]
        public async Task<ActionResult<DemographicProfile>> MatchProfile(
            [FromQuery] string cityId,
            [FromQuery] int age,
            [FromQuery] MaritalStatus maritalStatus,
            [FromQuery] int childrenCount)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cityId))
                {
                    return BadRequest("CityId is required");
                }

                var profiles = await _context.DemographicProfiles
                    .Where(p => p.CityId == cityId)
                    .ToListAsync();

                if (!profiles.Any())
                {
                    return NotFound($"No profiles found for city '{cityId}'");
                }

                var scoredProfiles = profiles.Select(p => new
                {
                    Profile = p,
                    Score = CalculateMatchScore(p, age, maritalStatus, childrenCount)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

                var bestMatch = scoredProfiles.First().Profile;

                return Ok(bestMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching profile for city {CityId}", cityId);
                return StatusCode(500, "An error occurred while matching the profile");
            }
        }

        private int CalculateMatchScore(DemographicProfile profile, int age, MaritalStatus maritalStatus, int childrenCount)
        {
            int score = 0;

            if (age >= profile.AgeMin && age <= profile.AgeMax)
            {
                score += 100;
            }
            else
            {
                int distance = Math.Min(Math.Abs(age - profile.AgeMin), Math.Abs(age - profile.AgeMax));
                score += Math.Max(0, 100 - (distance * 10));
            }

            if (profile.MaritalStatus == maritalStatus)
            {
                score += 50;
            }

            if (profile.ChildrenCount == childrenCount)
            {
                score += 50;
            }
            else
            {
                int distance = Math.Abs(profile.ChildrenCount - childrenCount);
                score += Math.Max(0, 50 - (distance * 10));
            }

            return score;
        }
    }
}
