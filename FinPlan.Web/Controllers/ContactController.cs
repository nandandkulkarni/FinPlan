using FinPlan.Web.Data;
using FinPlan.Web.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinPlan.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : MyControllerBase
    {
        private readonly FinPlanDbContext _db;

        public ContactController(FinPlanDbContext db)
        {
            _db = db;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveContact([FromBody] ContactSaveRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            try
            {
                var contactMessage = new ContactMessage
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Email = request.Email,
                    Message = request.Message,
                    UserGuid = request.UserGuid,
                    CreatedAt = GetEasternTime()
                };

                _db.ContactMessages.Add(contactMessage);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Contact message saved successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving contact message: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while saving your message" });
            }
        }
    }
}
