using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "I")]
    public class InstructorPostController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<InstructorPostController> _logger;

        public InstructorPostController(HelpmeContext context, ILogger<InstructorPostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult AddPost()
        {
            return View("Post");
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(int discussionID, string postContent)
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            _logger.LogInformation("AddPost method called with parameters: InstructorID={InstructorID}, discussionID={DiscussionID}, postContent={PostContent}", instructorId, discussionID, postContent);

            try
            {
                // Call the stored procedure to add the post
                await _context.Database.ExecuteSqlRawAsync($"EXEC Post2 @InstructorID = {instructorId} , @DiscussionID = {discussionID} , @Post = {postContent}");
                
                TempData["Message"] = "Post added successfully.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while adding the post.");
                TempData["Message"] = "An error occurred while adding the post.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("AddPost");
        }
    }
}