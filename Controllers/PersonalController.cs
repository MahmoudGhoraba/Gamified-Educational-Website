﻿using System.Data;
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
    [Authorize(Roles = "L")]
    public class PersonalController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<PersonalController> _logger;

        public PersonalController(HelpmeContext context, ILogger<PersonalController> logger)
        {
            _context = context;
            _logger = logger;
        }

       public IActionResult PersonalChoose()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId.HasValue)
            {
                ViewBag.LearnerID = learnerId.Value;
            }
            else
            {
                return View("Error");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLearner(string? firstname, string? lastname, string? country, string? cultural_background)
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            var learner = await _context.Learners.FindAsync(learnerId.Value);
            if (learner == null)
            {
                ViewBag.Message = "Learner not found.";
                return View("PersonalChoose");
            }
            if(firstname != null)
                learner.FirstName = firstname;
            if (lastname != null)
                learner.LastName = lastname;
            if(country != null)
                learner.Country = country;
            if(cultural_background != null)
                learner.CulturalBackground = cultural_background;
            
            ViewBag.Message = "Learner info updated successfully.";
            _context.Learners.Update(learner);
            await _context.SaveChangesAsync();
            return RedirectToAction("PersonalChoose");
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(string id, string field1, string field2, string field3)
        {
            if (!int.TryParse(id, out int profileId))
            {
                ViewBag.Message = "Invalid Profile ID.";
                return View("PersonalChoose");
            }

            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                _logger.LogInformation("Learner with prob.", learnerId);

                ViewBag.Message = "Invalid Learner ID.";
                return View("Error");
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == learnerId && p.ProfileId == profileId);
            _logger.LogInformation("Creating PersonalizationProfile with ID: {Id}, Preferred Content Type: {Field1}, Personality Type: {Field2}, Emotional State: {Field3}", id, field1, field2, field3);

            if (existingProfile != null)
            {
                ViewBag.Message = "Personal ID already exists.";
                return View("PersonalChoose");
            }

            // Create a new entry in the PersonalizationProfiles table
            var newProfile = new PersonalizationProfile
            {
                LearnerId = learnerId.Value,
                ProfileId = profileId,
                PreferedContentType = field1,
                PersonalityType = field2,
                EmotionalState = field3
            };

            _context.PersonalizationProfiles.Add(newProfile);
            await _context.SaveChangesAsync();
            TempData["profileId"] = profileId;
            return RedirectToAction("Personalized_Profile","LearnerPProfile");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            if (!int.TryParse(id, out int profileId))
            {
                ViewBag.Message = "Invalid Profile ID.";
                return View("PersonalChoose");
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == learnerId && p.ProfileId == profileId);

            if (existingProfile == null)
            {
                ViewBag.Message = "Personal ID does not exist.";
                return View("PersonalChoose");
            }

            // Delete the entry from the PersonalizationProfiles table
            _context.PersonalizationProfiles.Remove(existingProfile);
            await _context.SaveChangesAsync();

            return RedirectToAction("PersonalChoose");
        }

        [HttpPost]
        public async Task<IActionResult> Go(string id)
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            if (!int.TryParse(id, out int profileId))
            {
                ViewBag.Message = "Invalid Profile ID.";
                return View("PersonalChoose");
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == learnerId && p.ProfileId == profileId);

            if (existingProfile == null)
            {
                ViewBag.Message = "Personal ID does not exist.";
                return View("PersonalChoose");
            }
            TempData["profileId"] = profileId;
            // Redirect to another controller/action
            return RedirectToAction("Personalized_Profile", "LearnerPProfile");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId.HasValue)
            {
                var learner = await _context.Learners.FindAsync(learnerId.Value);
                if (learner != null)
                {
                    var page = await _context.SignupPages.FirstOrDefaultAsync(p => p.Email == learner.Email);
                    _context.Learners.Remove(learner);
                    await _context.SaveChangesAsync();
                    if (page != null)
                    {
                        _context.SignupPages.Remove(page);
                        await _context.SaveChangesAsync();
                    }
                    _logger.LogInformation("Learner with ID {LearnerID} deleted.", learnerId.Value);
                    HttpContext.Session.SetString("Message", "Account deleted.");
                }
                else
                {
                    _logger.LogWarning("Learner with ID {LearnerID} not found.", learnerId.Value);
                    HttpContext.Session.SetString("Message", "Account deleted.");
                }
            }
            else
            {
                _logger.LogWarning("LearnerID not found in TempData.");
                HttpContext.Session.SetString("Message", "LearnerID not found.");
            }

            return RedirectToAction("index","Home");
        }
        
        [HttpPost]
        public async Task<IActionResult> ViewScore( int AssessmentID)
        {
            var learnerId =HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }
            var scoreParam = new SqlParameter("@score", SqlDbType.Int)
                { Direction = ParameterDirection.Output };
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC ViewScore @LearnerID = {0}, @AssessmentID = {1}, @score = {2} OUTPUT", learnerId, AssessmentID, scoreParam);
            
            if (scoreParam.Value != DBNull.Value)
            {
                ViewData["Score"] = scoreParam.Value;
                return View();
            }
            else
            {
                return View();
            }
        }

        public async Task<IActionResult> Progress()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }
            var quests = await _context.QuestsPro.FromSqlRaw("EXEC QuestProgress @LearnerID = {0}", learnerId).ToListAsync();
            var badges = await _context.BadgesPro.FromSqlRaw("EXEC BadgeProgress @LearnerID = {0}", learnerId).ToListAsync();
            ViewBag.list = quests;
            ViewBag.badges = badges;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AssessmentAnalysis()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            var results = await _context.TakenAssessments
                .FromSqlRaw("Exec AssessmentAnalysis @LearnerID = {0}", learnerId)
                .ToListAsync();
            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> GoUpdate(string firstName, string lastName, DateTime birthDate, string country, string CulturalBackground)
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            // Call the stored procedure to update learner info
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC  @LearnerID = {0}, @FirstName = {1}, @LastName = {2}, @BirthDate = {3}, @Country = {4}",
                learnerId.Value, firstName, lastName, birthDate, country,CulturalBackground);

            if (result == 0)
            {
                ViewBag.Message = "Failed to update learner info.";
                return View("PersonalChoose");
            }

            HttpContext.Session.SetString("Message", "Learner info updated successfully.");
            return RedirectToAction("PersonalChoose");
        }

        [HttpPost]
        public IActionResult Profile()
        {
            
            // Redirect to another controller's action
            return RedirectToAction("ProfileDetails", "LearnerPProfile");
        }

        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorViewModel);
        }
        
        public async Task<IActionResult> CheckUpcomingGoals()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                ViewBag.noti = "LearnerID is not provided.";
                return View("Error");
            }
            
            int rowsAffected = await _context.Database.ExecuteSqlRawAsync("EXEC GoalReminder @LearnerID = {0}", learnerId);
            
            ViewBag.noti = rowsAffected > 0 
                ? "Reminder notification sent successfully for your upcoming learning goals." 
                : "All is fine! You are on track with your learning goals.";
            
            return View();
        }
        
        public async Task<IActionResult> ViewNotifications()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            // Fetch notifications for the specific learner using raw SQL
            var notifications = await _context.Notifications
                .FromSqlRaw("Exec ViewNot @LearnerID = {0}", learnerId)
                .ToListAsync();

            // Pass the list of notifications to the view
            return View(notifications);
        }
        
        [HttpPost]
        public async Task<IActionResult> MarkasRead(int notificationId)
        {
            
            var learnerId = HttpContext.Session.GetInt32("LearnerID");

            if (learnerId == null)
            {
                return View("Error");
            }
            // Fetch notifications for the specific learner using raw SQL
            var notifications = await _context.Database
                .ExecuteSqlRawAsync("EXEC NotificationUpdate @LearnerID = {0} , @NotificationID = {1} , @ReadStatus = 1", learnerId , notificationId);

            return RedirectToAction("ViewNotifications", "Personal");
        }
        
        [HttpGet]
        public async Task<IActionResult> Review()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            var takenAssessments = await _context.TakenAssessments
                .Where(ta => ta.LearnerId == learnerId.Value)
                .Include(ta => ta.Assessment)
                .ToListAsync();

            return View(takenAssessments);
        }
        
        public IActionResult SelectActivityForShare()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitEmotionalFeedback(int activityId, string emotionalState)
        {
            // Get the LearnerID from the session
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
    
            if (learnerId == null)
            {
                ViewBag.ErrorMessage = "You must be logged in as a learner to submit feedback.";
                return View("PersonalChoose");
            }

            if (string.IsNullOrEmpty(emotionalState))
            {
                ViewBag.ErrorMessage = "Please select an emotional state.";
                return View("SelectActivityForShare");
            }
            _logger.LogInformation("SubmitEmotionalFeedback method called with parameters: ActivityID={ActivityID}, LearnerID={LearnerID}, Timestamp={Timestamp}, EmotionalState={EmotionalState}", activityId, learnerId, DateTime.Now, emotionalState);
            try
            {
                // Call the stored procedure
                int r = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC ActivityEmotionalFeedback @ActivityID = {0}, @LearnerID = {1}, @Timestamp = {2}, @EmotionalState = {3}", activityId, learnerId.Value, DateTime.Now, emotionalState);
                if (r > 0)
                {
                    ViewBag.Success = "Emotional feedback submitted successfully!";
                }
                else
                {
                    ViewBag.ErrorMessage = "This is not an Activity You are in!";

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while submitting feedback.");
                ViewBag.ErrorMessage = "An error occurred while submitting your feedback. Please try again.";
            }

            return View();
        }
        public async Task<IActionResult> LeaderboardFilter()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }
            
            var leaderboard = await _context.Rankings
                .FromSqlRaw("EXEC LeaderboardFilter @LearnerID = {0}", learnerId)
                .ToListAsync();
            return View(leaderboard);
        }

    }
}
/* this is what i understand 
 [HttpPost]
public async Task<IActionResult> UpdateLearnerInfo(string preferedContentType, string emotionalState)
{
    var learnerId = TempData.Peek("LearnerID") as int?;
    if (learnerId == null)
    {
        return View("Error");
    }

    var learner = await _context.Learners.FindAsync(learnerId.Value);
    if (learner == null)
    {
        ViewBag.ErrorMessage = "Learner not found.";
        return View("PersonalChoose");
    }

    learner.Prefered_Content_Type = preferedContentType;
    learner.emotional_state = emotionalState;

    _context.Learners.Update(learner); // Mark the entity as modified
    await _context.SaveChangesAsync(); // Generate and execute the SQL UPDATE statement

    TempData["Message"] = "Learner info updated successfully.";
    return RedirectToAction("PersonalChoose");
}
 
 */




/* OR THIS???????????+
    [HttpPost]
        public async Task<IActionResult> UpdateLearnerInfo(string preferedContentType, string emotionalState)
        {
            var learnerId = TempData.Peek("LearnerID") as int?;
            if (learnerId == null)
            {
                return View("Error");
            }

            // Call the stored procedure to update learner info
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC ProfileUpdate @LearnerID = {0}, @PreferedContentType = {1}, @EmotionalState = {2}",
                learnerId.Value, preferedContentType, emotionalState);

            if (result == 0)
            {
                ViewBag.ErrorMessage = "Failed to update learner info.";
                return View("PersonalChoose");
            }

            TempData["Message"] = "Learner info updated successfully.";
            return RedirectToAction("PersonalChoose");
        }

    
    */