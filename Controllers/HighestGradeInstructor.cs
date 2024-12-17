using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    public class HighestGradeInstructorController : Controller
    {
        private readonly HelpmeContext _context;

        public HighestGradeInstructorController(HelpmeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> HighestGrades(CancellationToken cancellationToken)
        {
            var highestGrades = await _context.Assessments.FromSqlRaw("Exec HighestGrade").ToListAsync(cancellationToken);

            return View(highestGrades);
        }
    }
}
