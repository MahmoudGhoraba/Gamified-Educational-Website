using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spaghetti.Controllers
{
    public class LakshyController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<AdminController> _logger;

        public LakshyController(HelpmeContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult LearnerComp2()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string learnerId, string id)
        {
            if (!int.TryParse(learnerId, out int parsedLearnerId))
            {
                return Json(new { success = false, message = "Invalid Learner ID." });
            }

            if (!int.TryParse(id, out int profileId))
            {
                return Json(new { success = false, message = "Invalid Profile ID." });
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == parsedLearnerId && p.ProfileId == profileId);

            if (existingProfile == null)
            {
                return Json(new { success = false, message = "Personal ID does not exist." });
            }

            // Delete the entry from the PersonalizationProfiles table
            _context.PersonalizationProfiles.Remove(existingProfile);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Profile deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInstructorAccount(string instructorId)
        {
            if (!int.TryParse(instructorId, out int parsedInstructorId))
            {
                return Json(new { success = false, message = "Invalid Instructor ID." });
            }

            // Check if the InstructorID exists in the Instructors table
            var existingInstructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.InstructorId == parsedInstructorId);

            if (existingInstructor == null)
            {
                return Json(new { success = false, message = "Instructor ID does not exist." });
            }

            // Delete the entry from the Instructors table
            _context.Instructors.Remove(existingInstructor);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Instructor account deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLearnerAccount(string learnerAccountId)
        {
            if (!int.TryParse(learnerAccountId, out int parsedLearnerAccountId))
            {
                return Json(new { success = false, message = "Invalid Learner ID." });
            }

            // Check if the LearnerID exists in the Learners table
            var existingLearner = await _context.Learners
                .FirstOrDefaultAsync(l => l.LearnerId == parsedLearnerAccountId);

            if (existingLearner == null)
            {
                return Json(new { success = false, message = "Learner ID does not exist." });
            }

            // Delete the entry from the Learners table
            _context.Learners.Remove(existingLearner);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Learner account deleted successfully." });
        }

        // Add the following action methods here to complete the controller class max points missing
        [HttpPost]
        public async Task<IActionResult> AddActivity(int courseId, int moduleId, string activityType, string details , int maxPoints)
        {
            try
            {
                // Call the stored procedure
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC AddActivity @CourseID = {0}, @ModuleID = {1}, @ActivityType = {2}, @Details = {3}, @MaxPoints = {4}",
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
                    .FromSqlRaw("EXEC GetModuleDetails @CourseID = {0}", courseId)
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
        public async Task<IActionResult> ViewEnrolledCourses(int learnerId)
        {
            try
            {
                // Call the stored procedure
                var courses = await _context.Courses
                    .FromSqlRaw("EXEC EnrolledCourses @LearnerID = {0}", learnerId)
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
                    return Json(new { success = false, message = "No courses found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the courses.");
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
                var result = await _context.Database.ExecuteSqlRawAsync("EXEC PrerequisitesString @LearnerID = {0}, @CourseID = {1}, @message = {2} OUTPUT",
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
        public async Task<IActionResult> RegisterCourse(int learnerId, int courseId)
        {
            try
            {
                // Call the stored procedure to register the learner to the course
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC RegisterLearnerToCourse @LearnerID = {0}, @CourseID = {1}", learnerId, courseId);
                if (result > 0)
                {
                    return Json(new { success = true, message = "Learner registered to the course successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to register learner to the course." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the learner to the course.");
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
                    "EXEC RemoveCourse @CourseID = {0}", courseId);

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

        [HttpPost]
        public async Task<IActionResult> AddAssessment(int moduleID, int courseID, string type, int total_Marks, int passing_Marks, string criteria, float weightage, string description, string title)
        {
            try
            {
                
                var resultMessage = new SqlParameter("@ResultMessage", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC AddAssessment @ModuleID = {0}, @CourseID = {1}, @Type = {2}, @TotalMarks = {3}, @PassingMarks = {4}, @Criteria = {5}, @Weightage = {6}, @Description = {7}, @Title = {8}, @ResultMessage = {9} OUTPUT",
                    moduleID, courseID, type, total_Marks, passing_Marks, criteria, weightage, description, title, resultMessage);
                
                if (result > 0)
                {
                    return Json(new { success = true, message = resultMessage.Value });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add assessment." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the assessment.");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
    }
}