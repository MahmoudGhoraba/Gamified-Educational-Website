using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "I")]

    public class INController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<INController> _logger;

        public INController(HelpmeContext context, ILogger<INController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Info()
        {
            var email = TempData.Peek("Email") as string; // Use Peek to keep TempData for the next request
            if (email != null)
            {
                ViewBag.Email = email;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Values(string name, string latestQualification, string expertiseArea)
        {
            var tempEmail = TempData["Email"] as string; // Retrieve TempData
            if (string.IsNullOrEmpty(tempEmail))
            {
                return View("Error");
            }
            var instructor = new Instructor
            {
                Name = name,
                LatestQualification = latestQualification,
                ExpertiseArea = expertiseArea,
                Email = tempEmail
            };
            try
            {
                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the instructor.");
                return View("Error");
            }
            var instructorId = instructor.InstructorId;
            // You can now use the instructorId as needed
            HttpContext.Session.SetInt32("InstructorID", instructor.InstructorId);


            HttpContext.Session.SetString("UserRole", "I");
            return RedirectToAction("IDash", "InstructorDash");
        }
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorViewModel);
        }
    }
}
