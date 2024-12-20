using Microsoft.AspNetCore.Mvc;
using Spaghetti.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "I")]

public class IPathController : Controller
{

    private readonly HelpmeContext _context;
    private readonly ILogger<IPathController> _logger;

    public IPathController(HelpmeContext context, ILogger<IPathController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult CreatePath()
    {
        // Return the view for creating a path
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePath(int learnerID, int profileID, string completionStatus, string customContent, string adaptiveRules)
    {
        _logger.LogInformation("CreatePath method called with parameters: learnerID={LearnerID}, profileID={ProfileID}, completionStatus={CompletionStatus}, customContent={CustomContent}, adaptiveRules={AdaptiveRules}", learnerID, profileID, completionStatus, customContent, adaptiveRules);
        var instructorId = HttpContext.Session.GetInt32("InstructorID");
        if (instructorId == null)
        {
            return View("Error");
        }

        try
        {
            // Call the NewPathAsync method
            await _context.Database.ExecuteSqlRawAsync($"EXEC NewPath @LearnerID = {learnerID} , @ProfileID = {profileID} , @completion_status = '{completionStatus}' , @custom_content = '{customContent}' , @adaptiverules = '{adaptiveRules}'");

            ViewBag.Message = "Path created successfully.";
            ViewBag.type = "success";
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, "An error occurred while creating the path.");
            ViewBag.Message = "An error occurred while creating the path.";
            ViewBag.type = "error";
        }

        return View();
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