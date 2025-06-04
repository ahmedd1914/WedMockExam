using Microsoft.AspNetCore.Mvc;
using WedMockExam.Services.DTOs.Workplace;
using WedMockExam.Services.Interfaces.Workplace;
using WedMockExam.Web.Attributes;
using WedMockExam.Web.Models.ViewModels.Workplace;

namespace WedMockExam.Web.Controllers
{
    [Authorize]
    public class WorkplaceController : BaseController
    {
        private readonly IWorkplaceService _workplaceService;

        public WorkplaceController(
            IWorkplaceService workplaceService,
            ILogger<WorkplaceController> logger) : base(logger)
        {
            _workplaceService = workplaceService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var workplaces = await _workplaceService.GetAllWorkplacesAsync();
                var viewModel = new WorkplaceViewModel
                {
                    AllWorkplaces = workplaces,
                    FilteredWorkplaces = workplaces,
                    SelectedDate = DateTime.Today
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workplaces");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var workplace = await _workplaceService.GetWorkplaceByIdAsync(id);
                if (workplace == null)
                {
                    TempData["ErrorMessage"] = "Workplace not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new WorkplaceViewModel
                {
                    WorkplaceId = workplace.WorkplaceId,
                    Floor = workplace.Floor,
                    Zone = workplace.Zone,
                    HasMonitor = workplace.HasMonitor,
                    HasDocking = workplace.HasDocking,
                    HasWindow = workplace.HasWindow,
                    HasPrinter = workplace.HasPrinter,
                    SelectedDate = DateTime.Today
                };

                // Check availability for today
                viewModel.IsWorkplaceAvailable = await _workplaceService.IsWorkplaceAvailableAsync(id, DateTime.Today);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workplace details");
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                return View(new WorkplaceViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create workplace form");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkplaceViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var request = new WorkplaceRequestDto
                {
                    Floor = model.Floor,
                    Zone = model.Zone,
                    HasMonitor = model.HasMonitor,
                    HasDocking = model.HasDocking,
                    HasWindow = model.HasWindow,
                    HasPrinter = model.HasPrinter
                };

                var result = await _workplaceService.CreateWorkplaceAsync(request);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Workplace created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Failed to create workplace.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workplace");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var workplace = await _workplaceService.GetWorkplaceByIdAsync(id);
                if (workplace == null)
                {
                    TempData["ErrorMessage"] = "Workplace not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new WorkplaceViewModel
                {
                    WorkplaceId = workplace.WorkplaceId,
                    Floor = workplace.Floor,
                    Zone = workplace.Zone,
                    HasMonitor = workplace.HasMonitor,
                    HasDocking = workplace.HasDocking,
                    HasWindow = workplace.HasWindow,
                    HasPrinter = workplace.HasPrinter
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workplace for edit");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, WorkplaceViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var request = new WorkplaceRequestDto
                {
                    Floor = model.Floor,
                    Zone = model.Zone,
                    HasMonitor = model.HasMonitor,
                    HasDocking = model.HasDocking,
                    HasWindow = model.HasWindow,
                    HasPrinter = model.HasPrinter
                };

                var result = await _workplaceService.UpdateWorkplaceAsync(id, request);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Workplace updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Failed to update workplace.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workplace");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var success = await _workplaceService.DeleteWorkplaceAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Workplace deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete workplace.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workplace");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int id, DateTime date)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var isAvailable = await _workplaceService.IsWorkplaceAvailableAsync(id, date);
                return Json(new { isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking workplace availability");
                return Json(new { error = "Error checking availability" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(WorkplaceViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                // Get all workplaces first
                var allWorkplaces = await _workplaceService.GetAllWorkplacesAsync();
                _logger.LogInformation($"Total workplaces: {allWorkplaces.Count()}");
                
                // Get available workplaces for the selected date
                var availableWorkplaces = await _workplaceService.GetAvailableWorkplacesForDateAsync(model.SelectedDate);
                _logger.LogInformation($"Available workplaces for {model.SelectedDate}: {availableWorkplaces.Count()}");
                
                // Apply filters if any are set
                var filteredWorkplaces = availableWorkplaces;
                if (model.SearchFilters != null)
                {
                    // Only apply filters if they have values
                    if (model.SearchFilters.Floor > 0 || 
                        !string.IsNullOrWhiteSpace(model.SearchFilters.Zone) ||
                        model.SearchFilters.HasMonitor ||
                        model.SearchFilters.HasDocking ||
                        model.SearchFilters.HasWindow ||
                        model.SearchFilters.HasPrinter)
                    {
                        var searchResults = await _workplaceService.SearchWorkplacesAsync(model.SearchFilters);
                        _logger.LogInformation($"Search results before availability filter: {searchResults.Count()}");
                        
                        // Combine search results with available workplaces
                        filteredWorkplaces = availableWorkplaces
                            .Where(aw => searchResults.Any(sr => sr.WorkplaceId == aw.WorkplaceId))
                            .ToList();
                        _logger.LogInformation($"Final filtered workplaces: {filteredWorkplaces.Count()}");
                    }
                }

                var viewModel = new WorkplaceViewModel
                {
                    AllWorkplaces = allWorkplaces,
                    FilteredWorkplaces = filteredWorkplaces,
                    SearchFilters = model.SearchFilters ?? new WorkplaceFilterRequestDto(),
                    SelectedDate = model.SelectedDate
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching workplaces");
                return View("Error");
            }
        }
    }
}
