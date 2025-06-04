using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WedMockExam.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger;
        }

        protected int? GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        protected IActionResult RequireUserId()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("No user ID found in session");
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        protected IActionResult RequireUserId(int requestedUserId)
        {
            var userId = GetUserId();
            if (!userId.HasValue || userId.Value != requestedUserId)
            {
                _logger.LogWarning("User ID mismatch or not found in session");
                return RedirectToAction("Login", "Account");
            }
            return null;
        }
    }
} 