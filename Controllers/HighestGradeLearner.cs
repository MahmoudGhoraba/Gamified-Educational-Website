using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    public class HighestGradeLearnerController : Controller
    {
        private readonly HelpmeContext _context;

        public HighestGradeLearnerController(HelpmeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> HighestGrades(CancellationToken cancellationToken)
        {
            var highestGrades = await _context.Assessments.FromSqlRaw("Exec HighestGrade").ToListAsync(cancellationToken);

            ViewBag.ReturnValue = highestGrades.Count;
            return View(highestGrades);
        }
    }
}
