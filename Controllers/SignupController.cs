using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    public class SignupController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<SignupController> _logger;

        public SignupController(HelpmeContext context, ILogger<SignupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index2()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index2(string email, string pw, string role)
        {
            // Ensure email and password are not null or empty
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(role))
            {
                return Json(new { success = false, message = "Email, password, and role are required." });
            }

            // Check if the user already exists
            var existingUser = await _context.SignupPages.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "User already exists." });
            }

            // Create a new user
            var newUser = new SignupPage
            {
                Email = email,
                Password = pw,
                Role = role
            };

            TempData["Email"] = email;
            
            _context.SignupPages.Add(newUser);
            await _context.SaveChangesAsync();
            


            // Create claims and sign in the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };


            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Json(new { success = true, role = role });
        }
    }
}