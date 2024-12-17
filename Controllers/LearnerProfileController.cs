using Microsoft.AspNetCore.Mvc;
using Spaghetti.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "L")]

    public class LearnerProfileController : Controller
    {
        private readonly HelpmeContext _context;

        public LearnerProfileController(HelpmeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ProfileDetails()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");

            if (learnerId == null)
            {
                return BadRequest("LearnerID is not provided.");
            }

            var learner = await _context.Learners.FindAsync(learnerId.Value);
            if (learner == null)
            {
                return NotFound();
            }

            return View(learner);
        }
    }
}