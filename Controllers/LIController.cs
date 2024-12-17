using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "L")]
    public class LIController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<LIController> _logger;

        public LIController(HelpmeContext context, ILogger<LIController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Values()
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
        public async Task<IActionResult> Values(string firstName, string lastName, string gender, DateTime birthDate, string country ,  IFormFile? profilePictureFile)
        {
            var email = TempData["Email"] as string; // Retrieve TempData
            
            if (string.IsNullOrEmpty(email))
            {
                return View("Error");
            }

            var learner = new Learner
            {
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                BirthDate = birthDate,
                Country = country,
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
                        return View(learner);
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
                learner.ProfilePicture = profilePicture;
            }

            try
            {
                _context.Learners.Add(learner);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the learner.");
                return View("Error");
            }

            var learnerId = learner.LearnerId;

            // You can now use the learnerId as needed
            HttpContext.Session.SetInt32("LearnerID", learner.LearnerId);


            HttpContext.Session.SetString("UserRole", "L");
            return RedirectToAction("PersonalChoose", "Personal");
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