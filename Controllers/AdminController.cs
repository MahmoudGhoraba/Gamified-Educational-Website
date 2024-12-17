﻿using System.Diagnostics;
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
