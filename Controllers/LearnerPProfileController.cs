using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "L")]
    public class LearnerPProfileController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<LearnerPProfileController> _logger;

        public LearnerPProfileController(HelpmeContext context, ILogger<LearnerPProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddGoal(int goalId, string goalValue, string status, DateTime deadline, string description)
        {
            var learnerId = HttpContext.Session.GetInt32("LearnerID");
            if (learnerId == null)
            {
                return View("Error");
            }

            try
            {
                // Execute the stored procedure to add the goal

                // Insert into the Learning_Goal table
                var learningGoal = new LearningGoal
                {
                    Id = goalId,
                    Status = status,
                    Deadline = deadline,
                    Description = description
                };
                
                _context.LearningGoals.Add(learningGoal);
                await _context.SaveChangesAsync();
                
                var result =await _context.Database.ExecuteSqlRawAsync("EXEC AddGoal @LearnerID = {0}, @GoalID = {1}", learnerId, goalId);

                if(result >0){
                    HttpContext.Session.SetString("Message", "Goal added successfully.");
                    HttpContext.Session.SetString("MessageType", "success");
                }
                else{
                    HttpContext.Session.SetString("Message", "An error occurred while connecting the goal to learner.");
                    HttpContext.Session.SetString("MessageType", "error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the goal.");
                HttpContext.Session.SetString("Message", "An error occurred while adding the goal.");
                HttpContext.Session.SetString("MessageType", "error");
                return View("Error");
            }

            await _context.Database.ExecuteSqlRawAsync($"EXEC AddGoal @LearnerID = {learnerId} , @GoalID = {goalId}");

            return RedirectToAction("Personalized_Profile");
        }

        public async Task<IActionResult> Personalized_Profile()
        {
            ViewBag.Message = HttpContext.Session.GetString("Message");
            ViewBag.MessageType = HttpContext.Session.GetString("MessageType");

            // Clear the message after displaying it
            HttpContext.Session.Remove("Message");
            HttpContext.Session.Remove("MessageType");
            int learnerId = HttpContext.Session.GetInt32("LearnerID") ?? 0;
            int profileId = TempData["profileId"] as int? ?? 0;
            var learner = await _context.PersonalizationProfiles.FindAsync(learnerId,profileId);
            return View(learner);
        }

        [HttpPost]
        public IActionResult ProfileDetails()
        {
            // Redirect to another controller's action
            return RedirectToAction("ProfileDetails", "LearnerProfile");
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