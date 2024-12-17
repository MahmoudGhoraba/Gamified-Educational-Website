using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "L")]

    public class LearnersPathsController : Controller
    {
        private readonly HelpmeContext _context;

        public LearnersPathsController(HelpmeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AllPaths()
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");

            if (learnerId == null)
            {
                return BadRequest("LearnerID is not provided.");
            }

            var currentPaths = await _context.LearningPaths
                .FromSqlRaw("EXEC CurrentPath @LearnerID = {0}", learnerId)
                .ToListAsync();     
            
            return View(currentPaths);
        }
    }
}