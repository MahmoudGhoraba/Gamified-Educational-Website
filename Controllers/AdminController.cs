using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "A")]
    public class AdminController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(HelpmeContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Page()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var email = TempData.Peek("Email") as string; // Use Peek to keep TempData for the next request
            if (email != null)
            {
                _logger.LogError("dsfhdsjsbjdf.");
                ViewBag.Email = email;
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(string firstName, string lastName, string gender,IFormFile? profilePictureFile)
        {
            var email = TempData["Email"] as string; // Retrieve TempData

            if (string.IsNullOrEmpty(email))
            {
                return View("Error");
            }

            var admin = new Admin
            {
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                Email = email
            };

            if (ModelState.IsValid)
            {

                string profilePicture = null;

                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    var fileExtension = Path.GetExtension(profilePictureFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfilePicture",
                            "Only images (.jpg, .jpeg, .png, .gif) are allowed.");
                        return View(admin);
                    }

                    var uploadsFolder = Path.Combine("wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePictureFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePictureFile.CopyToAsync(fileStream);
                    }

                    profilePicture = "/uploads/" + uniqueFileName;
                }

                admin.ProfilePicture = profilePicture;
            }

            try
            {
                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the learner.");
                return View("Error");
            }

            var adminId = admin.AdminId;

            // You can now use the learnerId as needed
            HttpContext.Session.SetInt32("AdminID", admin.AdminId);


            HttpContext.Session.SetString("UserRole", "A" );
            return RedirectToAction("Page", "Admin");
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

        
        [HttpPost]
        public async Task<IActionResult> ProfileDetails()
        {
            var adminId = HttpContext.Session.GetInt32("AdminID");
            if (adminId == null)
            {
                return BadRequest("AdminId is not provided.");
            }

            var admin = await _context.Admins.FindAsync(adminId.Value);
            
            if (admin == null)
            {
                return NotFound();
            }
            return View(admin);
        }
        
        public IActionResult SelectLearnerForNotifications()
        {
            return View();
        }
        

        public async Task<IActionResult> ViewNotifications(int learnerId)
        {
            // Fetch notifications for the specific learner using raw SQL
            var notifications = await _context.Notifications
                .FromSqlRaw("Exec ViewNot @LearnerID = {0}", learnerId)
                .ToListAsync();

            ViewBag.LearnerID = learnerId;
            // Pass the list of notifications to the view
            return View(notifications);
        }
        
        [HttpPost]
        public async Task<IActionResult> MarkasRead(int? learnerId,int notificationId)
        {
            if (learnerId == null)
            {
                return View("Error");
            }
            // Fetch notifications for the specific learner using raw SQL
            var notifications = await _context.Database
                .ExecuteSqlRawAsync("EXEC NotificationUpdate @LearnerID = {0} , @NotificationID = {1} , @ReadStatus = 1", learnerId , notificationId);

            return RedirectToAction("SelectLearnerForNotifications", "Admin");
        }
        
        [HttpPost]
        public async Task<IActionResult> EmotionalTrendAnalysis(int courseID, int moduleID, DateTime timePeriod)
        {
            
                var results = await _context.EmotionalFeedbacks.FromSqlRaw(
                    "EXEC EmotionalTrendAnalysis @CourseID = {0}, @ModuleID = {1}, @TimePeriod = {2}",
                    courseID, moduleID, timePeriod).ToListAsync();
                

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
