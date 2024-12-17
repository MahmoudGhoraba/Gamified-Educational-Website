using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Create(string id, string field1, string field2, string field3)
        {
            if (!int.TryParse(id, out int profileId))
            {
                ViewBag.ErrorMessage = "Invalid Profile ID.";
                return View("PersonalChoose");
            }

            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                _logger.LogInformation("Learner with prob.", learnerId);

                ViewBag.ErrorMessage = "Invalid Learner ID.";
                return View("Error");
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == learnerId && p.ProfileId == profileId);
            _logger.LogInformation("Creating PersonalizationProfile with ID: {Id}, Preferred Content Type: {Field1}, Personality Type: {Field2}, Emotional State: {Field3}", id, field1, field2, field3);

            if (existingProfile != null)
            {
                ViewBag.ErrorMessage = "Personal ID already exists.";
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

            return RedirectToAction("CreateSuccess");
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
                ViewBag.ErrorMessage = "Invalid Profile ID.";
                return View("PersonalChoose");
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == learnerId && p.ProfileId == profileId);

            if (existingProfile == null)
            {
                ViewBag.ErrorMessage = "Personal ID does not exist.";
                return View("PersonalChoose");
            }

            // Delete the entry from the PersonalizationProfiles table
            _context.PersonalizationProfiles.Remove(existingProfile);
            await _context.SaveChangesAsync();

            return RedirectToAction("DeleteSuccess");
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
                ViewBag.ErrorMessage = "Invalid Profile ID.";
                return View("PersonalChoose");
            }

            // Check if the PersonalID exists in the PersonalizationProfiles table
            var existingProfile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerId == learnerId && p.ProfileId == profileId);

            if (existingProfile == null)
            {
                ViewBag.ErrorMessage = "Personal ID does not exist.";
                return View("PersonalChoose");
            }

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
                    _context.Learners.Remove(learner);
                    await _context.SaveChangesAsync();
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

            return RedirectToAction("PersonalChoose");
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UpdateLearnerInfo(string firstName, string lastName, DateTime birthDate, string country)
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            // Call the stored procedure to update learner info
            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC ProfileUpdate @LearnerID = {0}, @FirstName = {1}, @LastName = {2}, @BirthDate = {3}, @Country = {4}",
                learnerId.Value, firstName, lastName, birthDate, country);

            if (result == 0)
            {
                ViewBag.ErrorMessage = "Failed to update learner info.";
                return View("PersonalChoose");
            }

            HttpContext.Session.SetString("Message", "Learner info updated successfully.");
            return RedirectToAction("PersonalChoose");
        }

        [HttpPost]
        public IActionResult Profile()
        {
            // Redirect to another controller's action
            return RedirectToAction("ProfileDetails", "LearnerProfile");
        }

        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorViewModel);
        }

        public IActionResult CreateSuccess()
        {
            return View();
        }

        public IActionResult DeleteSuccess()
        {
            return View();
        }

        public IActionResult GoSuccess()
        {
            return View();
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