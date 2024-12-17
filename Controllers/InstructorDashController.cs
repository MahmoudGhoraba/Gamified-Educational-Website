using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "I")]
    public class InstructorDashController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<InstructorDashController> _logger;

        public InstructorDashController(HelpmeContext context, ILogger<InstructorDashController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult AddAssesment()
{
    return RedirectToAction("AddAssesment", "InstructorPost");
}


public IActionResult RedirectToInstructorCreateFourm()
{
    return RedirectToAction("CreateFourm", "InstructorPost");
}

        [HttpPost]
        public async Task<IActionResult> UpdateInstructor(string name, string latest_qualifications, string expertise_area)
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            var instructor = await _context.Instructors.FindAsync(instructorId.Value);
            if (instructor == null)
            {
                ViewBag.ErrorMessage = "Instructor not found.";
                return View("IDash");
            }

            instructor.Name = name;
            instructor.LatestQualification = latest_qualifications;
            instructor.ExpertiseArea = expertise_area;

            _context.Instructors.Update(instructor);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Instructor info updated successfully.";
            return RedirectToAction("IDash");
        }

        public IActionResult IDash()
        {
            return View();
        }

        public IActionResult ShowProfile()
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            return RedirectToAction("IProfileDetails", "InstructorProfile");
        }
        public IActionResult CreatePathRedirect()
        {
            // Redirect to the CreatePath action in the IPathController
            return RedirectToAction("CreatePath", "IPath");
        }
        public IActionResult RedirectToInstructorPost()
        {
            return RedirectToAction("AddPost", "InstructorPost");
        }
        
        [HttpGet]
        public IActionResult ReviewAssessments()
        {
            // Retrieve all taken assessments from the database
            var takenAssessments = _context.TakenAssessments
                .Include(ta => ta.Assessment)
                .Include(ta => ta.Learner)
                .ToList();

            return View(takenAssessments);
        }

        [HttpPost]
        public async Task<IActionResult> EmotionalTrendAnalysis(int courseID, int moduleID, DateTime timePeriod)
        {
            
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }
            
            var results = await _context.EmotionalFeedbacks.FromSqlRaw(
                "EXEC EmotionalTrendAnalysisIns @CourseID = {0}, @ModuleID = {1}, @TimePeriod = {2}, @InstructorID = {3}",
                courseID, moduleID, timePeriod,instructorId).ToListAsync();
                

            return View(results);  // Pass the results to your view
            
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