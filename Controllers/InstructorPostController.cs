using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "I")]
    public class InstructorPostController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<InstructorPostController> _logger;

        public InstructorPostController(HelpmeContext context, ILogger<InstructorPostController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult AddPost()
        {
            return View("Post");
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(int discussionID, string postContent)
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            _logger.LogInformation("AddPost method called with parameters: InstructorID={InstructorID}, discussionID={DiscussionID}, postContent={PostContent}", instructorId, discussionID, postContent);

            try
            {
                // Call the stored procedure to add the post
                await _context.Database.ExecuteSqlRawAsync($"EXEC Post2 @InstructorID = {instructorId} , @DiscussionID = {discussionID} , @Post = {postContent}");
                
                TempData["Message"] = "Post added successfully.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while adding the post.");
                TempData["Message"] = "An error occurred while adding the post.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("AddPost");
        }
        [HttpPost]
        public async Task<IActionResult> CreateFourm(int ModuleID, int CourseID, string title, string description)
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            _logger.LogInformation("CreateFourm method called with parameters: ModuleID={ModuleID}, CourseID={CourseID}, title={title}, description={description}", ModuleID, CourseID, title, description);

            try
            {
                // Call the stored procedure to add the post
                await _context.Database.ExecuteSqlRawAsync("EXEC CreateDiscussion @ModuleID = {0}, @CourseID = {1}, @Title = {2}, @Description = {3}",
                    ModuleID, CourseID, title, description);

                TempData["Message"] = "Post added successfully.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while adding the post.");
                TempData["Message"] = "An error occurred while adding the post.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("CreateFourm");
        }



        [HttpPost]
        public async Task<IActionResult> AddAssesment(int ModuleID, int CourseID, string Type, int Total_Marks, int Passing_Marks, string Criteria, float Weightage, string Description, string Title)
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorID");
            if (instructorId == null)
            {
                return View("Error");
            }

            _logger.LogInformation("AddAssesment method called with parameters: ModuleID={ModuleID}, CourseID={CourseID}, Type={Type}, Total_Marks={Total_Marks}, Passing_Marks={Passing_Marks}, Criteria={Criteria}, Weightage={Weightage}, Description={Description}, Title={Title}", ModuleID, CourseID, Type, Total_Marks, Passing_Marks, Criteria, Weightage, Description, Title);

            try
            {
                // Call the stored procedure to add the assessment
                await _context.Database.ExecuteSqlRawAsync("EXEC AddAssessment @ModuleID = {0}, @CourseID = {1}, @Type = {2}, @Total_Marks = {3}, @Passing_Marks = {4}, @Criteria = {5}, @Weightage = {6}, @Description = {7}, @Title = {8}",
                    ModuleID, CourseID, Type, Total_Marks, Passing_Marks, Criteria, Weightage, Description, Title);

                TempData["Message"] = "Assessment added successfully.";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while adding the assessment.");
                TempData["Message"] = "An error occurred while adding the assessment.";
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("AddAssesment");
        }
    }
}