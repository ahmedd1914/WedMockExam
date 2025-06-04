using Microsoft.AspNetCore.Mvc;
using WedMockExam.Services.Interfaces.PreferredLocation;
using WedMockExam.Services.Interfaces.Workplace;
using WedMockExam.Services.DTOs.PreferredLocation;
using Microsoft.Extensions.Logging;
using WedMockExam.Web.Models.ViewModels.PreferredLocation;
using WedMockExam.Web.Attributes;

namespace WedMockExam.Web.Controllers
{
    [Authorize]
    public class PreferredLocationController : BaseController
    {
        private readonly IPreferredLocationService _preferredLocationService;
        private readonly IWorkplaceService _workplaceService;

        public PreferredLocationController(
            IPreferredLocationService preferredLocationService,
            IWorkplaceService workplaceService,
            ILogger<PreferredLocationController> logger) : base(logger)
        {
            _preferredLocationService = preferredLocationService;
            _workplaceService = workplaceService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var viewModel = new PreferredLocationViewModel
                {
                    UserId = userId
                };

                // Get user's preferred locations
                var preferredLocations = await _preferredLocationService.GetUserPreferredWorkplacesAsync(userId);
                viewModel.UserPreferredLocations = preferredLocations;

                // Get available workplaces
                var availableWorkplaces = await _workplaceService.GetAvailableWorkplacesAsync();
                viewModel.AvailableWorkplaces = availableWorkplaces;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading preferred locations");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(PreferredLocationViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                var userId = GetUserId().Value;
                var request = new AddPreferredLocationDto
                {
                    UserId = userId,
                    WorkplaceId = model.WorkplaceId,
                    PreferenceRank = model.PreferenceRank
                };

                try
                {
                    var result = await _preferredLocationService.AddPreferredWorkplaceAsync(request);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Preferred location added successfully.";
                        TempData.Remove("ErrorMessage");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Unable to add preferred location. Please try again.";
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding preferred location");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove(PreferredLocationViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var request = new RemovePreferredLocationDto
                {
                    UserId = userId,
                    WorkplaceId = model.WorkplaceId
                };

                var success = await _preferredLocationService.RemovePreferredWorkplaceAsync(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Preferred location removed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to remove preferred location.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing preferred location");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRank(PreferredLocationViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                if (!ModelState.IsValid)
                {
                    return View("Index", model);
                }

                var userId = GetUserId().Value;
                var request = new UpdatePreferenceRankDto
                {
                    UserId = userId,
                    WorkplaceId = model.WorkplaceId,
                    NewRank = model.PreferenceRank
                };

                var success = await _preferredLocationService.UpdatePreferenceRankAsync(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Preference rank updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to update preference rank.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preference rank");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int userId)
        {
            try
            {
                var requireUserIdResult = RequireUserId(userId);
                if (requireUserIdResult != null) return requireUserIdResult;

                var canAddMore = await _preferredLocationService.CanAddMorePreferredLocationsAsync(userId);
                return Json(new { canAddMore });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking preferred location availability");
                return Json(new { error = "Error checking availability" });
            }
        }
    }
}
