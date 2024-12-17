using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "I")]

    public class LeaderboardsController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<LeaderboardsController> _logger;

        public LeaderboardsController(HelpmeContext context, ILogger<LeaderboardsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> LeaderboardRank(int id)
        {
            var leaderboards = await _context.Assessments.FromSqlRaw("Exec LeaderboardRank @LeaderboardID = {0}", id)
                .ToListAsync();
            return View(leaderboards);
        }

    }
}
