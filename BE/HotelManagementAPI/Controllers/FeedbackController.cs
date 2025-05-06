using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;
using System.Security.Claims;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public FeedbackController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Feedback
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetFeedbacks()
        {
            return await _context.Feedbacks
                .Include(f => f.User)
                .ThenInclude(u => u.Customer)
                .Select(f => new
                {
                    CustomerName = f.User.Customer.CustomerName,
                    f.Comment,
                    f.TimeComment
                })
                .OrderByDescending(f => f.TimeComment)
                .ToListAsync();
        }

        // POST: api/Feedback
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<Feedback>> CreateFeedback(FeedbackRequest request)
        {
            if (string.IsNullOrEmpty(request.Comment))
            {
                return BadRequest("Comment cannot be empty");
            }

            // Get user ID from authenticated user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            var feedback = new Feedback
            {
                UserID = userId,
                Comment = request.Comment,
                TimeComment = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Feedback submitted successfully",
                feedback.TimeComment
            });
        }

        // DELETE: api/Feedback
        [HttpDelete]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteFeedback(FeedbackDeleteRequest request)
        {
            // Get user ID from authenticated user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Find the feedback by user ID and timestamp
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.UserID == userId && f.TimeComment == request.TimeComment);

            if (feedback == null)
            {
                return NotFound("Feedback not found or does not belong to you");
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class FeedbackRequest
    {
        public string Comment { get; set; }
    }

    public class FeedbackDeleteRequest
    {
        public DateTime TimeComment { get; set; }
    }
}
