using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    public class AchievementsController : Controller
    {
        private readonly HelpmeContext _context;

        public AchievementsController(HelpmeContext context)
        {
            _context = context;
        }

        // GET: Achievements
        public async Task<IActionResult> Index()
        {
            var x = _context.Achievements.Include(a => a.Badge).Include(a => a.Learner);
            return View(await x.ToListAsync());
        }

        // GET: Achievements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var achievement = await _context.Achievements
                .Include(a => a.Badge)
                .Include(a => a.Learner)
                .FirstOrDefaultAsync(m => m.AchievementId == id);
            if (achievement == null)
            {
                return NotFound();
            }

            return View(achievement);
        }

        // GET: Achievements/Create
        public IActionResult Create()
        {
            ViewData["BadgeID"] = new SelectList(_context.Badges, "BadgeID", "BadgeID");
            ViewData["LearnerID"] = new SelectList(_context.Learners, "LearnerID", "LearnerID");
            return View();
        }

        // POST: Achievements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AchievementID,LearnerID,BadgeID,Description,Date_Earned,Type")] Achievement achievement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(achievement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BadgeID"] = new SelectList(_context.Badges, "BadgeID", "BadgeID", achievement.BadgeId);
            ViewData["LearnerID"] = new SelectList(_context.Learners, "LearnerID", "LearnerID", achievement.LearnerId);
            return View(achievement);
        }

        // GET: Achievements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null)
            {
                return NotFound();
            }
            ViewData["BadgeID"] = new SelectList(_context.Badges, "BadgeID", "BadgeID", achievement.BadgeId);
            ViewData["LearnerID"] = new SelectList(_context.Learners, "LearnerID", "LearnerID", achievement.LearnerId);
            return View(achievement);
        }

        // POST: Achievements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AchievementID,LearnerID,BadgeID,Description,Date_Earned,Type")] Achievement achievement)
        {
            if (id != achievement.AchievementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(achievement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AchievementExists(achievement.AchievementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BadgeID"] = new SelectList(_context.Badges, "BadgeID", "BadgeID", achievement.BadgeId);
            ViewData["LearnerID"] = new SelectList(_context.Learners, "LearnerID", "LearnerID", achievement.LearnerId);
            return View(achievement);
        }

        // GET: Achievements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var achievement = await _context.Achievements
                .Include(a => a.Badge)
                .Include(a => a.Learner)
                .FirstOrDefaultAsync(m => m.AchievementId == id);
            if (achievement == null)
            {
                return NotFound();
            }

            return View(achievement);
        }

        // POST: Achievements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement != null)
            {
                _context.Achievements.Remove(achievement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AchievementExists(int id)
        {
            return _context.Achievements.Any(e => e.AchievementId == id);
        }

        // GET: Achievements/AddAchievement
        public IActionResult addAchievement()
        {
            // Assuming you have a list of badges in your database
            ViewData["Badges"] = _context.Badges.ToList();
            return View();
        }

        // POST: Achievements/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addAchievement(Achievement achievement)
        {
            if (ModelState.IsValid)
            {
                // Assuming that learnerID and BadgeID are being passed correctly and that learnerID is set elsewhere
                achievement.DateEarned = DateTime.Now;

                // Add the achievement to the database
                _context.Add(achievement);
                await _context.SaveChangesAsync();

                // Redirect or show a confirmation message
                return RedirectToAction("Index", "Achievements");  // Redirect to a list of achievements
            }

            // If the model is not valid, reload the list of badges
            ViewData["Badges"] = _context.Badges.ToList();
            return View(achievement);  // Return the form with validation errors
        }
        public async Task<IActionResult> TrackAchievements(int learnerId)
        {
            try
            {
                // Get the learner's achievements
                var achievements = await _context.Achievements
    .Where(a => a.LearnerId == learnerId)
    .Include(a => a.Badge)  // Ensure Badge data is included
    .ToListAsync();

                // Pass the achievements to the view
                return View(achievements);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while fetching achievements: {ex.Message}";
                return View(new List<Achievement>());
            }
        }
    }


}
