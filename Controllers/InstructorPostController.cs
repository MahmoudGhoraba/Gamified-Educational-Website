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
                int res = await _context.Database.ExecuteSqlRawAsync("EXEC Post2 @InstructorID = {0} , @DiscussionID = {1} , @Post = {2}", instructorId, discussionID, postContent);
                if (res > 0)
                {
                    TempData["Message"] = "Post added successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Post not added successfully.";
                    TempData["MessageType"] = "success";
                }
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
        [HttpPost]
        public async Task<IActionResult> CreateFourm(int ModuleID, int CourseID, string title, string description)
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            _logger.LogInformation("CreateFourm method called with parameters: ModuleID={ModuleID}, CourseID={CourseID}, title={title}, description={description}", ModuleID, CourseID, title, description);

            try
            {
                // Call the stored procedure to create the discussion
                await _context.Database.ExecuteSqlRawAsync("EXEC CreateDiscussion @ModuleID = {0}, @CourseID = {1}, @Title = {2}, @Description = {3}",
                    ModuleID, CourseID, title, description);
                ViewBag.m = "Forum created successfully.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while creating the forum.");
                ViewBag.m = "An error occurred while creating the forum.";
                TempData["MessageType"] = "error";
            }

            return View();
        }
    }
}