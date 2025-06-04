using Microsoft.AspNetCore.Mvc;
using WedMockExam.Services.DTOs.Reservation;
using WedMockExam.Services.Interfaces.Reservation;
using WedMockExam.Services.Interfaces.Workplace;
using WedMockExam.Web.Attributes;
using WedMockExam.Web.Models.ViewModels.Reservation;

namespace WedMockExam.Web.Controllers
{
    [Authorize]
    public class ReservationController : BaseController
    {
        private readonly IReservationService _reservationService;
        private readonly IWorkplaceService _workplaceService;

        public ReservationController(
            IReservationService reservationService,
            IWorkplaceService workplaceService,
            ILogger<ReservationController> logger) : base(logger)
        {
            _reservationService = reservationService;
            _workplaceService = workplaceService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var viewModel = new ReservationViewModel
                {
                    UserId = userId,
                    SelectedDate = DateTime.Today
                };

                // Get user's reservations
                var userReservations = await _reservationService.GetUserReservationsAsync(userId);
                var workplaceIds = userReservations.Select(r => r.WorkplaceId).Distinct().ToList();
                var workplaces = await _workplaceService.GetAllWorkplacesAsync();
                var workplaceDict = workplaces.ToDictionary(w => w.WorkplaceId);
                foreach (var reservation in userReservations)
                {
                    if (workplaceDict.TryGetValue(reservation.WorkplaceId, out var workplace))
                    {
                        reservation.Workplace = workplace;
                    }
                }
                viewModel.UserReservations = userReservations;

                // Get reservations for today
                var dateReservations = await _reservationService.GetReservationsForDateAsync(DateTime.Today);
                var dateWorkplaceIds = dateReservations.Select(r => r.WorkplaceId).Distinct().ToList();
                foreach (var reservation in dateReservations)
                {
                    if (workplaceDict.TryGetValue(reservation.WorkplaceId, out var workplace))
                    {
                        reservation.Workplace = workplace;
                    }
                }
                viewModel.DateReservations = dateReservations;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservations");
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

                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null)
                {
                    TempData["ErrorMessage"] = "Reservation not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Verify the reservation belongs to the current user
                var userId = GetUserId().Value;
                if (reservation.UserId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to view this reservation.";
                    return RedirectToAction(nameof(Index));
                }

                // Populate Workplace
                reservation.Workplace = await _workplaceService.GetWorkplaceByIdAsync(reservation.WorkplaceId);

                var viewModel = new ReservationViewModel
                {
                    ReservationId = reservation.ReservationId,
                    UserId = reservation.UserId,
                    WorkplaceId = reservation.WorkplaceId,
                    ReservationDate = reservation.ReservationDate,
                    IsQuickReservation = reservation.IsQuickReservation,
                    AvailableWorkplaces = await _workplaceService.GetAllWorkplacesAsync(),
                };
                viewModel.UserReservations = new List<ReservationResponseDto> { reservation };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservation details");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                var userId = GetUserId().Value;
                var viewModel = new ReservationViewModel 
                { 
                    UserId = userId,
                    AvailableWorkplaces = await _workplaceService.GetAllWorkplacesAsync()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create reservation form");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReservationViewModel model)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Ensure the user can only create reservations for themselves
                var userId = GetUserId().Value;
                if (model.UserId != userId)
                {
                    TempData["ErrorMessage"] = "You can only create reservations for yourself.";
                    return RedirectToAction(nameof(Index));
                }

                var request = new ReservationRequestDto
                {
                    UserId = userId,
                    WorkplaceId = model.WorkplaceId,
                    ReservationDate = model.ReservationDate
                    
                };

                var result = await _reservationService.CreateReservationAsync(request);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Reservation created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Unable to create reservation. Please check availability and try again.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return View("Error");
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
                var request = new QuickReservationRequestDto
                {
                    UserId = userId,
                    WorkplaceId = workplaceId
                };

                var result = await _reservationService.CreateQuickReservationAsync(request);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Quick reservation created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to make quick reservation. Please check availability and try again.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making quick reservation");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                // Verify the reservation belongs to the current user
                var userId = GetUserId().Value;
                var reservation = await _reservationService.GetReservationByIdAsync(id);
                if (reservation == null || reservation.UserId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to cancel this reservation.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _reservationService.CancelReservationAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Reservation cancelled successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to cancel reservation.";
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int userId, DateTime date)
        {
            try
            {
                var requireUserIdResult = RequireUserId();
                if (requireUserIdResult != null) return requireUserIdResult;

                // Verify the user is checking their own availability
                var currentUserId = GetUserId().Value;
                if (userId != currentUserId)
                {
                    return Json(new { error = "You can only check your own availability" });
                }

                var canReserve = await _reservationService.CanUserReserveForDateAsync(userId, date);
                return Json(new { canReserve });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking reservation availability");
                return Json(new { error = "Error checking availability" });
            }
        }
    }
}
