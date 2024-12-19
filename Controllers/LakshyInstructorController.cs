using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spaghetti.Controllers
{
    public class LakshyInstructorController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<AdminController> _logger;

        public LakshyInstructorController(HelpmeContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult InstructorLaksh()
        {
            return View();
        }

        // Add the following action methods here to complete the controller class max points missing
        [HttpPost]
        public async Task<IActionResult> AddActivity(int courseId, int moduleId, string activityType, string details,
            int maxPoints)
        {
            try
            {
                // Call the stored procedure
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC NewActivity @CourseID = {0}, @ModuleID = {1}, @activitytype = {2}, @instructiondetails = {3}, @maxpoints = {4}",
                    courseId, moduleId, activityType, details, maxPoints);

                if (result > 0)
                {
                    return Json(new { success = true, message = "Activity added successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add activity." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the activity.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewModules(int courseId)
        {
            try
            {
                // Call the stored procedure
                var modules = await _context.Modules
                    .FromSqlRaw("EXEC ModuleDifficulty @CourseID = {0}", courseId)
                    .ToListAsync();

                if (modules != null && modules.Count > 0)
                {
                    var moduleData = modules.Select(m => new
                    {
                        m.ModuleId,
                        m.CourseId,
                        m.Title,
                        m.Difficulty,
                        m.ContentUrl
                    }).ToList();

                    return Json(new { success = true, data = moduleData });
                }
                else
                {
                    return Json(new { success = false, message = "No modules found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the modules.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewPreviousCourses(int learnerId)
        {
            try
            {
                // Call the stored procedure
                var courses = await _context.Courses
                    .FromSqlRaw("EXEC EnrolledCoursesPrevious @LearnerID = {0}", learnerId)
                    .ToListAsync();
                if (courses != null && courses.Count > 0)
                {
                    var courseData = courses.Select(c => new
                    {
                        c.CourseId,
                        c.Title,
                        c.LearningObjective,
                        c.CreditPoints,
                        c.DifficultyLevel,
                        c.Description
                    }).ToList();

                    return Json(new { success = true, data = courseData });
                }
                else
                {
                    return Json(new { success = false, message = "No previous courses found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the previous courses.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckPrerequisites(int learnerId, int courseId)
        {
            try
            {
                // Call the stored procedure to check prerequisites
                var messageParam = new SqlParameter("@message", SqlDbType.VarChar, 100)
                    { Direction = ParameterDirection.Output };
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC PrerequisitesString @LearnerID = {0}, @CourseID = {1}, @message = {2} OUTPUT",
                    learnerId, courseId, messageParam);

                return Json(new { success = result > 0, message = messageParam.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking prerequisites.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCourse(int courseId)
        {
            try
            {
                // Call the stored procedure to remove the course
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC CourseRemoveHelp @CourseID = {0}", courseId);

                if (result > 0)
                {
                    return Json(new { success = true, message = "Course removed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to remove course." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing the course.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
    }
}