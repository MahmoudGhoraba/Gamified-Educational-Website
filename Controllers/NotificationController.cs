using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spaghetti.Models;

namespace Spaghetti.Controllers
{
    [Authorize(Roles = "A")]
    public class NotificationsController : Controller
    {
        private readonly HelpmeContext _context;
        private readonly ILogger<AdminController> _logger;

        public NotificationsController(HelpmeContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
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
}