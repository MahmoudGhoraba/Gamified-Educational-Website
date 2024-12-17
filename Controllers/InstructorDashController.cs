using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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

        public async Task<IActionResult> AssessmentNot(int? NotificationID, string message, string urgencylevel, int? LearnerID)
        {
            DateTime s = DateTime.Now;
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC AssessmentNot @NotificationID = {0},@Timestamp = {1},@Message = {2}, @UrgencyLevel = {3}, @LearnerID = {4}",
                NotificationID, s, message, urgencylevel, LearnerID);

            if (result > 0)
            {
                TempData["Message"] = "Notification sent successfully.";
                return View("IDash");
            }
            else
            {
                TempData["Message"] = "Notification not sent successfully.";
                return View("IDash");
            }
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
        public async Task<IActionResult> AddAssessmentToModule(int moduleId, int assessmentId)
        {
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC AddAssessmentToModule @ModuleID = {0}, @AssessmentID = {1}",
                    moduleId, assessmentId);

                if (result > 0)
                {
                    return Json(new { success = true, message = "Assessment added to module successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add assessment to module." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the assessment to the module.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        public async Task<IActionResult> AssessmentAnalyticsResult(int moduleId, int courseId)
        {
            // Call the stored procedure to get the assessment analytics result
            var messageParam = new SqlParameter("@message", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var result = await _context.Database.ExecuteSqlRawAsync("Exec AssessmentAnalytics @CourseID = {0}, @ModuleID = {1}, @AverageScore = {2} OUTPUT",
                courseId, moduleId, messageParam);

            ViewBag.integer = messageParam.Value;
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int learnerId, int assessmentId, int points)
        {
            var returnValue = await _context.Database.ExecuteSqlRawAsync(
                "EXEC GradeUpdate @LearnerID = {0}, @AssessmentID = {1}, @Points = {2}", learnerId, assessmentId,
                points);
                if (returnValue > 0)
                {
                    // Success
                    TempData["Message"] = "Grade updated successfully.";
                }
                else
                {
                    // Failure
                    TempData["Error"] = "Failed to update grade.";
                }

            return View();
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

        [HttpPost]
        public IActionResult AddDeadline(int questId, DateTime deadline)
        {
            // Find the Collaborative entity associated with the questId
            var collaborative = _context.Collaboratives
                .FirstOrDefault(c => c.QuestId == questId);

            if (collaborative == null)
            {
                return NotFound();
            }

            // Update the Deadline
            collaborative.Deadline = deadline;

            // Save changes to the database
            _context.Update(collaborative);
            _context.SaveChanges();

            // Redirect to the AllQuests view (or wherever you want after updating)
            return RedirectToAction("AllQuests2", "Quests");
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