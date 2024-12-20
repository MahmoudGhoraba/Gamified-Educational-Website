using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    public class QuestsController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<QuestsController> _logger;


        public QuestsController(HelpmeContext context, ILogger<QuestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Quests
        public async Task<IActionResult> Index(int learnerId)
        {
            try
            {
                // Get the list of all quests
                var quests = await _context.Quests.ToListAsync();

                // Get the list of quests that the learner has already joined
                var joinedQuests = await _context.LearnersCollaborations
                                                  .Where(lc => lc.LearnerId == learnerId)
                                                  .Select(lc => lc.QuestId)
                                                  .ToListAsync();

                // Pass both the quests and the joined quest IDs to the view
                ViewBag.JoinedQuests = joinedQuests;

                return View(quests);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View();
            }
        }


        // GET: Quests/Details/5
        [HttpGet("Quests/DetailsF/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            var learnerID = HttpContext.Session.GetInt32("LearnerID");
            if (id == null)
            {
                return NotFound();
            }

            var quest = await _context.Quests
                .FirstOrDefaultAsync(m => m.QuestId == id);
            if (quest == null)
            {
                return NotFound();
            }
            
            ViewBag.LearnerID = learnerID;
            return View(quest);
        }

        // Route for instructors
        [HttpGet("Quests/DetailsForInstructor/{questId?}")]
        public async Task<IActionResult> DetailsForInstructor(int? questId, int instructorID)
        {
            if (questId == null)
            {
                return NotFound();
            }

            var quest = await _context.Quests
                .Include(q => q.Collaborative)  // Ensure the Collaborative entity is loaded
                .FirstOrDefaultAsync(m => m.QuestId == questId);

            if (quest == null)
            {
                return NotFound();
            }

            // Store InstructorID in TempData
            TempData["InstructorID"] = instructorID;

            return View(quest);  // Pass the quest with the loaded Collaborative entity to the view
        }
        // Display the quest creation form


        // GET: Quest/Create
        // GET: Quest/Create
        public IActionResult Create()
        {
            ViewBag.Skills = _context.Skills.ToList(); // Populate skills from the database
            return View();
        }
        
        public async Task<IActionResult> SearchQuests(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                // If no search query is entered, return all quests
                ViewBag.AllQuests = await _context.Quests.ToListAsync();
            }
            else
            {
                // Filter quests based on the search query in title or criteria
                ViewBag.AllQuests = await _context.Quests
                    .Where(q => q.Title.Contains(searchQuery) || q.Criteria.Contains(searchQuery))
                    .ToListAsync();
            }
            

            return View("AllQuests");  // Redirect to the same view after searching
        }
        // POST: Quest/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quest model, DateTime deadline, int maxParticipants)
        {
            if (ModelState.IsValid)
            {
                // Combine all the parameters for the stored procedure
                var parameters = new[]
                {
            new SqlParameter("@Difficulty_Level", model.DifficultyLevel),
            new SqlParameter("@Criteria", model.Criteria),
            new SqlParameter("@Description", model.Description),
            new SqlParameter("@Title", model.Title),
            new SqlParameter("@Maxnumparticipants", maxParticipants),
            new SqlParameter("@Deadline", deadline)
        };

                // Execute the stored procedure in a single call
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[CollaborativeQuest] @Difficulty_Level, @Criteria, @Description, @Title, @Maxnumparticipants, @Deadline",
                    parameters
                );

                // Redirect to the list of all quests after successful insertion
                return RedirectToAction("AllQuests2");
            }

            // If validation fails, return to the form with errors
            return View(model);
        }



        //delete
        public async Task<IActionResult> ManageQuests(string difficultyLevel, string criteria)
        {
            var quests = _context.Quests.AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(difficultyLevel))
            {
                quests = quests.Where(q => q.DifficultyLevel == difficultyLevel);
            }

            if (!string.IsNullOrEmpty(criteria))
            {
                quests = quests.Where(q => q.Criteria.Contains(criteria)); // Search by criteria
            }

            // Fetch quests from database
            var allQuests = await quests.ToListAsync();
            ViewBag.AllQuests = allQuests; // Make sure this is not null



            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteSelected(List<int> questIds)
        {
            try
            {
                var questsToDelete = await _context.Quests
                    .Where(q => questIds.Contains(q.QuestId))
                    .ToListAsync();

                if (questsToDelete.Any())
                {
                    _context.Quests.RemoveRange(questsToDelete); // Bulk delete
                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"{questsToDelete.Count} quests deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "No quests found for deletion.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting quests: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageQuests)); // Redirect back to ManageQuests
        }
        // GET: Quests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quest = await _context.Quests.FindAsync(id);
            if (quest == null)
            {
                return NotFound();
            }
            return View(quest);
        }

        // POST: Quests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestID,Difficulty_Level,Criteria,Description,Title")] Quest quest)
        {
            if (id != quest.QuestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestExists(quest.QuestId))
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
            return View(quest);
        }

        // GET: Quests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quest = await _context.Quests
                .FirstOrDefaultAsync(m => m.QuestId == id);
            if (quest == null)
            {
                return NotFound();
            }

            return View(quest);
        }

        // POST: Quests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quest = await _context.Quests.FindAsync(id);
            if (quest != null)
            {
                _context.Quests.Remove(quest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestExists(int id)
        {
            return _context.Quests.Any(e => e.QuestId == id);
        }
        // Action to Join a Quest
      [HttpPost]
public async Task<IActionResult> JoinQuest(int questId)
{
    try
    {
        // Get the learner's ID from the session
        var learnerId = HttpContext.Session.GetInt32("LearnerID");
        
        int result =await  _context.Database.ExecuteSqlRawAsync("Exec JoinQuest @LearnerID = {0}, @QuestID = {1}",learnerId,questId);
        
        if(result>0){
            TempData["Message"] = "You successfully joined the quest!";
        }
        else
        {
            TempData["Message"] = "Quest Full.";
        }
    }
    catch (Exception ex)
    {
        // Log the full exception
        Console.WriteLine($"Error: {ex.InnerException?.Message ?? ex.Message}");
        TempData["Error"] = $"An error occurred: {ex.InnerException?.Message ?? ex.Message}";
    }

    return RedirectToAction("AvailableQuests");
}

public IActionResult AddDeadline(int questId)
{
    _logger.LogInformation(questId.ToString());
    ViewBag.QuestID = questId;
    return View();
}

[HttpPost]
public async Task<IActionResult> AddDeadline(int questId, DateTime deadline)
{
    _logger.LogInformation(questId.ToString());
    _logger.LogInformation(deadline.ToString());
    // Find the Collaborative entity associated with the questId
    int collaborative = await _context.Database.ExecuteSqlRawAsync("Exec DeadlineUpdate @QuestID = {0}, @Deadline = {1}",questId,deadline);
    if(collaborative>0){
        TempData["Message"] = "Deadline added successfully!";
        return RedirectToAction("allquests2");
    }
    else
    {
        TempData["Error"] = "An error occurred while adding the deadline.";
        return View();
    }

    return RedirectToAction("AllQuests2");
}

        public async Task<IActionResult> AvailableQuests()
        {
            try
            {
                
                // Get the learner's ID from the session
                var learnerId = HttpContext.Session.GetInt32("LearnerID");
                if (learnerId == null)
                {
                    TempData["Error"] = "Learner ID is not available.";
                    return RedirectToAction("Profile","Personal");
                }
                // Fetch IDs and completion status of quests the learner has already joined
                var joinedQuests = await _context.LearnersCollaborations
                    .Where(lc => lc.LearnerId == learnerId)
                    .Join(_context.Quests, lc => lc.QuestId, q => q.QuestId, (lc, q) => new
                    {
                        q.QuestId,
                        q.Title,
                        q.Description,
                        lc.CompletionStatus
                    })
                    .ToListAsync();

                // Fetch all quests
                var allQuests = await _context.Quests.ToListAsync();

                // Separate quests into joined and available lists
                var availableQuests = allQuests.Where(q => !joinedQuests.Any(jq => jq.QuestId == q.QuestId)).ToList();

                // Pass data to the view
                ViewBag.LearnerID = learnerId; // Learner's ID
                ViewBag.JoinedQuests = joinedQuests; // List of joined quests with completion status

                return View(availableQuests); // Pass available quests to the view
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while fetching quests: {ex.Message}";
                return View(new List<Quest>());
            }
        }


        public async Task<IActionResult> ActiveQuests(int learnerId)
        {
            try
            {
                // Get IDs of quests the learner has joined
                var activeQuestIds = await _context.LearnersCollaborations
                    .Where(lc => lc.LearnerId == learnerId)
                    .Select(lc => lc.QuestId)
                    .ToListAsync();

                // Fetch quest details for these IDs
                var activeQuests = await _context.Quests
                    .Where(q => activeQuestIds.Contains(q.QuestId))
                    .ToListAsync();

                ViewBag.LearnerID = learnerId; // Pass learner ID for navigation
                return View(activeQuests);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while fetching active quests: {ex.Message}";
                return View(new List<Quest>());
            }
        }

     public async Task<IActionResult> Participants(int questId)
{
    try
    {
        var learnerId = HttpContext.Session.GetInt32("LearnerID");
        // Fetch participants for the specified quest
        var participants = await _context.LearnersCollaborations
            .Where(lc => lc.QuestId == questId)
            .Include(lc => lc.Learner) // Join with Learners table
            .Select(lc => lc.Learner) // Fetch learner details
            .ToListAsync();

        // Pass both QuestID and LearnerID to the view
        ViewBag.QuestID = questId;
        ViewBag.LearnerID = learnerId;
        return View(participants);
    }
    catch (Exception ex)
    {
        TempData["Error"] = $"An error occurred: {ex.Message}";
        return View(new List<Learner>());
    }
}
        [HttpGet]
        public async Task<IActionResult> AllQuests()
        {
            var collaborativeQuests = await _context.Quests
                .Where(q => q.Collaborative != null)
                .ToListAsync();

            var skillMasteryQuests = await _context.Quests
                .Where(q => q.SkillMastery != null)
                .ToListAsync();

            ViewBag.CollaborativeQuests = collaborativeQuests;
            ViewBag.SkillMasteryQuests = skillMasteryQuests;

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AllQuests2()
        {
            var collaborativeQuests = await _context.Quests
                .Where(q => q.Collaborative != null)
                .ToListAsync();

            var skillMasteryQuests = await _context.Quests
                .Where(q => q.SkillMastery != null)
                .ToListAsync();

            ViewBag.CollaborativeQuests = collaborativeQuests;
            ViewBag.SkillMasteryQuests = skillMasteryQuests;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuest(int questId)
        {
            var quest = await _context.Quests.FindAsync(questId);
            if (quest == null)
            {
                TempData["Error"] = "Quest not found!";
                return RedirectToAction("AllQuests2");
            }

            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Quest deleted successfully!";
            return RedirectToAction("AllQuests2");
        }


        // Action to view quest details
        [HttpGet]
        public async Task<IActionResult> QuestDetails(int questId)
        {
            var quest = await _context.Quests
                .FirstOrDefaultAsync(q => q.QuestId == questId);

            if (quest == null)
            {
                TempData["Error"] = "Quest not found!";
                return RedirectToAction("AllQuests");
            }

            return View(quest);
        }
       




    }
}