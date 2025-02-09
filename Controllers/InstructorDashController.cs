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
        
        [HttpPost]
        public async Task<IActionResult> AddAssessment(int moduleID, int courseID, string type, int total_Marks, int passing_Marks, string criteria, float weightage, string description, string title)
        {
            try
            {
                
                var resultMessage = new SqlParameter("@ResultMessage", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC addAssesment @ModuleID = {0}, @CourseID = {1}, @Type = '1', @Total_Marks = 100, @Passing_Marks = 50, @Criteria = 'high', @Weightage = 20, @Description = 'help', @Title = 'high', @R = {9} OUTPUT",
                    moduleID, courseID, type, total_Marks, passing_Marks, criteria, weightage, description, title, resultMessage);
                
                if (result > 0)
                {
                    ViewBag.Message = "Assessment added successfully.";
                    return View();
                }
                else
                {
                    ViewBag.Message = "Failed to add assessment.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the assessment.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
    

        public IActionResult RedirectToInstructorCreateFourm()
        {
            return RedirectToAction("CreateFourm", "InstructorPost");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInstructor(string? name, string? latest_qualifications, string? expertise_area)
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
            if(name != null)
                instructor.Name = name;
            if(latest_qualifications != null)
                instructor.LatestQualification = latest_qualifications;
            if(expertise_area != null)
                instructor.ExpertiseArea = expertise_area;

            _context.Instructors.Update(instructor);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Instructor info updated successfully.";
            return RedirectToAction("IDash");
        }

        public IActionResult IDash()
        {
            var iID = HttpContext.Session.GetInt32("InstructorID");
            if (iID.HasValue)
            {
                ViewBag.ID = iID.Value;
            }
            
            return View();
        }

        public async Task<IActionResult> AssessmentNot(int? NotificationID, string message, string urgencylevel, int? LearnerID)
        {
            DateTime s = DateTime.Now;
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC AssessmentNot @NotificationID = {0},@Timestamp = {1},@Message = {2}, @UrgencyLevel = {3}, @LearnerID = {4}",
                    NotificationID, s, message, urgencylevel, LearnerID);

                if (result > 0)
                {
                    ViewBag.Message = "Notification sent successfully.";
                    return View();
                }
                else
                {
                    ViewBag.Message = "Notification not sent successfully.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the notification.");
                ViewBag.Message = "An error occurred. (The system will never forgive you)";
                return View();
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

            if(messageParam.Value != DBNull.Value){
                ViewBag.integer = messageParam.Value;            }
            else{
                ViewBag.ErrorMessage = "Failed to get the assessment analytics result.";
            }
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
        public async Task<IActionResult> Modules()
        {
            var modules = await _context.Modules.ToListAsync();
            if (modules == null)
            {
                return NotFound();
            }

            return View(modules);
        }
  
        [HttpGet]
        public IActionResult AddAssessments()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddAssessments(int moduleID, int courseID, string type, int total_Marks, int passing_Marks, string criteria, float weightage, string description, string title)
        {
            try
            {
                
                var resultMessage = new SqlParameter("@ResultMessage", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC addAssesment @ModuleID = {0}, @CourseID = {1}, @Type = {2}, @Total_Marks = {3}, @Passing_Marks = {4}, @Criteria = {5}, @Weightage = {6}, @Description = {7}, @Title = {8}, @R = {9} OUTPUT",
                    moduleID, courseID, type, total_Marks, passing_Marks, criteria, weightage, description, title, resultMessage);
                
                if (result > 0)
                {
                    ViewBag.Message = "Assessment added successfully.";
                    return View();
                }
                else
                {
                    ViewBag.Message = "Failed to add assessment.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the assessment.");
                return Json(new { success = false, message = "An error occurred." });
            }
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