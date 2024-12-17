using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    public class HomeController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(HelpmeContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            _logger.LogInformation("Attempting to authenticate user with email: {Email}", email);

            // Ensure email and password are not null or empty
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email or password is empty.");
                return View("Error");
            }

            // Authenticate the user
            var user = await _context.SignupPages
                .FirstOrDefaultAsync(u => u.Email.Equals(email) && u.Password.Equals(password));

            if (user == null)
            {
                _logger.LogWarning("Authentication failed for email: {Email}", email);
                // Authentication failed
                return View("Error");
            }

            _logger.LogInformation("User authenticated successfully with email: {Email}", email);

            // Create claims and sign in the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("UserRole", user.Role);


            // Save the LearnerID or InstructorID in TempData
            if (user.Role == "L")
            {
                var learner = await _context.Learners.FirstOrDefaultAsync(l => l.Email == email);
                if (learner != null)
                {
                    HttpContext.Session.SetInt32("LearnerID", learner.LearnerId);
                }
            }
            else if (user.Role == "I")
            {
                var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Email == email);
                if (instructor != null)
                {
                    HttpContext.Session.SetInt32("InstructorID", instructor.InstructorId);
                }
            }

            // Redirect based on role
            switch (user.Role)
            {
                case "A":
                    return RedirectToAction("Page", "Admin");
                case "L":
                    return RedirectToAction("PersonalChoose", "Personal");
                case "I":
                    return RedirectToAction("IDash", "InstructorDash");
                default:
                    _logger.LogWarning("Invalid role for email: {Email}", email);
                    return View("Error");
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
        public IActionResult Privacy()
        {
            return View();
        }
    }
}