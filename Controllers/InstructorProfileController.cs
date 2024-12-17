using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "I")]

    public class InstructorProfileController : Controller
    {
        private readonly HelpmeContext _context;

        public InstructorProfileController(HelpmeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IProfileDetails()
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return BadRequest("LearnerID is not provided.");
            }

            var instructor = await _context.Instructors.FindAsync(instructorId.Value);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }
    }
}