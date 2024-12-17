using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "A")]

    public class AdminPostController : Controller
    {

        private readonly HelpmeContext _context;
        private readonly ILogger<AdminPostController> _logger;

        public AdminPostController(HelpmeContext context, ILogger<AdminPostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult CreateForum()
        {
            return View("CreateForum");
        }

        public async Task<IActionResult> CreateForum(int ModuleID, int CourseID, string title, string description)
        {
            var instructorId = HttpContext.Session.GetInt32("AdminID");
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
                TempData["Message"] = "Forum created successfully.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while creating the forum.");
                TempData["Message"] = "An error occurred while creating the forum.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("Page","Admin");
        }
    }
}
