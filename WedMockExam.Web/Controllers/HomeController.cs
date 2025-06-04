using System.Diagnostics;
using WedMockExam.Web.Models;
using WedMockExam.Web.Models.ViewModels.Home;
using WedMockExam.Services.Interfaces.Workplace;
using WedMockExam.Services.Interfaces.Reservation;
using WedMockExam.Services.Interfaces.PreferredLocation;
using WedMockExam.Services.DTOs.Workplace;
using WedMockExam.Services.DTOs.Reservation;
using WedMockExam.Services.DTOs.PreferredLocation;
using WedMockExam.Web.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace WedMockExam.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IWorkplaceService _workplaceService;
        private readonly IReservationService _reservationService;
        private readonly IPreferredLocationService _preferredLocationService;

        public HomeController(
            ILogger<HomeController> logger,
            IWorkplaceService workplaceService,
            IReservationService reservationService,
            IPreferredLocationService preferredLocationService) : base(logger)
        {
            _workplaceService = workplaceService;
            _reservationService = reservationService;
            _preferredLocationService = preferredLocationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var viewModel = new HomeViewModel
                {
                    SelectedDate = DateTime.Today,
                    WorkplaceFilters = new WorkplaceFilterRequestDto()
                };

                // Get user's preferred workplaces that are available next working day
                var preferredLocations = await _preferredLocationService.GetUserPreferredWorkplacesAvailableNextDayAsync(userId);
                viewModel.PreferredWorkplaces = preferredLocations.Select(pl => 
                    _workplaceService.GetWorkplaceByIdAsync(pl.WorkplaceId).Result).ToList();

                // Get user's current reservations
                viewModel.UserReservations = await _reservationService.GetUserReservationsAsync(userId);

                // Check if user can add more preferred locations
                viewModel.CanAddMorePreferredLocations = await _preferredLocationService.CanAddMorePreferredLocationsAsync(userId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }


        [HttpPost]
        public async Task<IActionResult> MakeReservation(HomeViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var reservationRequest = new ReservationRequestDto
                {
                    UserId = userId,
                    WorkplaceId = model.NewReservation.WorkplaceId,
                    ReservationDate = model.SelectedDate
                };

                var result = await _reservationService.CreateReservationAsync(reservationRequest);
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Unable to make reservation. Please check availability and try again.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Reservation created successfully!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making reservation");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickReservation(int workplaceId)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var quickReservationRequest = new QuickReservationRequestDto
                {
                    UserId = userId,
                    WorkplaceId = workplaceId
                };

                var result = await _reservationService.CreateQuickReservationAsync(quickReservationRequest);
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Unable to make quick reservation. Please check availability and try again.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Quick reservation created successfully!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making quick reservation");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
