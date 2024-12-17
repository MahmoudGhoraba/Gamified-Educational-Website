using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "L")]
    public class DiscussionController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<DiscussionController> _logger;

        public DiscussionController(HelpmeContext context, ILogger<DiscussionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult AddPost()
        {
            ViewBag.Message = HttpContext.Session.GetString("Message");
            ViewBag.MessageType = HttpContext.Session.GetString("MessageType");

            // Clear the message after displaying it
            HttpContext.Session.Remove("Message");
            HttpContext.Session.Remove("MessageType");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(int discussionId, string postContent)
        {
            _logger.LogInformation("AddPost method in discussioncontroller called with parameters: discussionID={DiscussionID}, postContent={PostContent}", discussionId, postContent);

            try
            {
                var learnerId = HttpContext.Session.GetInt32("LearnerID");

                // Call the stored procedure to add the post
                await _context.Database.ExecuteSqlRawAsync($"EXEC Post @LearnerID = {learnerId} , @DiscussionID = {discussionId} , @Post = {postContent}");

                
                HttpContext.Session.SetString("Message", "Post added successfully.");
                HttpContext.Session.SetString("MessageType", "success");
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while adding the post.");
                HttpContext.Session.SetString("Message", "An error occurred while adding the post.");
                HttpContext.Session.SetString("MessageType", "error");
            }

            return RedirectToAction("AddPost");
        }
    }
}