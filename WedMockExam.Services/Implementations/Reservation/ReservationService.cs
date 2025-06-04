using WedMockExam.Repository.Interfaces.Reservation;
using WedMockExam.Services.DTOs.Reservation;
using WedMockExam.Services.Interfaces.Reservation;
using WedMockExam.Services.Helpers;
using Microsoft.Extensions.Logging;
using WedMockExam.Models;

namespace WedMockExam.Services.Implementations.Reservation
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(IReservationRepository reservationRepository, ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _logger = logger;
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            var reservation = await _reservationRepository.RetrieveAsync(reservationId);
            if (reservation == null)
            {
                return false;
            }

            var update = new ReservationUpdate
            {
                UserId = reservation.UserId,
                WorkplaceId = reservation.WorkplaceId,
                BookingDate = reservation.BookingDate,
                IsCancelled = true
            };

            return await _reservationRepository.UpdateAsync(reservationId, update);
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(ReservationRequestDto request)
        {
            var dateOnly = request.ReservationDate.Date;
            _logger.LogInformation($"Attempting to create reservation for UserId={request.UserId}, WorkplaceId={request.WorkplaceId}, Date={dateOnly:yyyy-MM-dd}");

            // Check for any reservation (cancelled or not) for this user and date
            var filter = new ReservationFilter { UserId = request.UserId, BookingDate = dateOnly };
            WedMockExam.Models.Reservation existingReservation = null;
            await foreach (var res in _reservationRepository.RetrieveCollectionAsync(filter))
            {
                existingReservation = res;
                break;
            }

            if (existingReservation != null)
            {
                _logger.LogWarning($"Existing reservation found: Id={existingReservation.ReservationId}, IsCancelled={existingReservation.IsCancelled}, WorkplaceId={existingReservation.WorkplaceId}");
                // Always update the existing reservation, regardless of workplace or cancellation status
                var updateResult = await _reservationRepository.UpdateAsync(existingReservation.ReservationId, new ReservationUpdate
                {
                    UserId = existingReservation.UserId,
                    WorkplaceId = request.WorkplaceId,
                    BookingDate = dateOnly,
                    IsCancelled = false
                });
                _logger.LogInformation($"Update result for reservation Id={existingReservation.ReservationId}: {updateResult}");
                if (updateResult)
                {
                    return await GetReservationByIdAsync(existingReservation.ReservationId);
                }
                _logger.LogError($"Failed to update reservation Id={existingReservation.ReservationId}");
                return null;
            }

            // Validate before insert
            if (!ReservationValidationHelper.IsDateValidForReservation(dateOnly))
            {
                _logger.LogWarning("Date is not valid for reservation.");
                return null;
            }
            if (!await CanUserReserveForDateAsync(request.UserId, dateOnly))
            {
                _logger.LogWarning("User cannot reserve for this date (already has a reservation).");
                return null;
            }
            if (!await IsWorkplaceAvailableForDateAsync(request.WorkplaceId, dateOnly))
            {
                _logger.LogWarning("Workplace is not available for this date.");
                return null;
            }

            var reservation = new Models.Reservation
            {
                UserId = request.UserId,
                WorkplaceId = request.WorkplaceId,
                BookingDate = dateOnly
            };

            var id = await _reservationRepository.CreateAsync(reservation);
            _logger.LogInformation($"Inserted new reservation with Id={id}");
            return await GetReservationByIdAsync(id);
        }

        public async Task<ReservationResponseDto> GetReservationByIdAsync(int reservationId)
        {
            var reservation = await _reservationRepository.RetrieveAsync(reservationId);
            return reservation != null ? MapToResponseDto(reservation) : null;
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetUserReservationsAsync(int userId)
        {
            var filter = new ReservationFilter { UserId = userId };
            var reservations = new List<ReservationResponseDto>();
            
            await foreach (var reservation in _reservationRepository.RetrieveCollectionAsync(filter))
            {
                if (!reservation.IsCancelled)
                {
                    reservations.Add(MapToResponseDto(reservation));
                }
            }
            
            return reservations;
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsForDateAsync(DateTime date)
        {
            var filter = new ReservationFilter { BookingDate = date };
            var reservations = new List<ReservationResponseDto>();
            
            await foreach (var reservation in _reservationRepository.RetrieveCollectionAsync(filter))
            {
                if (!reservation.IsCancelled)
                {
                    reservations.Add(MapToResponseDto(reservation));
                }
            }
            
            return reservations;
        }
        public async Task<ReservationResponseDto> CreateQuickReservationAsync(QuickReservationRequestDto request)
        {
            var nextWorkingDay = ReservationValidationHelper.GetNextWorkingDay();
            var reservationRequest = new ReservationRequestDto
            {
                UserId = request.UserId,
                WorkplaceId = request.WorkplaceId,
                ReservationDate = nextWorkingDay
            };

            return await CreateReservationAsync(reservationRequest);
        }

        public async Task<IEnumerable<ReservationResponseDto>> SearchReservationsAsync(ReservationFilterDto filter)
        {
            var reservationFilter = new ReservationFilter
            {
                UserId = filter.UserId,
                WorkplaceId = filter.WorkplaceId,
                BookingDate = filter.BookingDate
            };

            var reservations = new List<ReservationResponseDto>();
            await foreach (var reservation in _reservationRepository.RetrieveCollectionAsync(reservationFilter))
            {
                if (filter.BookingDate.HasValue && reservation.BookingDate > filter.BookingDate.Value)
                {
                    continue;
                }
               
                if (!reservation.IsCancelled)
                {
                    reservations.Add(MapToResponseDto(reservation));
                }
            }
            
            return reservations;
        }

        public async Task<bool> CanUserReserveForDateAsync(int userId, DateTime date)
        {
            var filter = new ReservationFilter 
            { 
                UserId = userId,
                BookingDate = date
            };

            await foreach (var reservation in _reservationRepository.RetrieveCollectionAsync(filter))
            {
                if (!reservation.IsCancelled) // Only block if not cancelled
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> IsWorkplaceAvailableForDateAsync(int workplaceId, DateTime date)
        {
            var filter = new ReservationFilter 
            { 
                WorkplaceId = workplaceId,
                BookingDate = date
            };

            var count = 0;
            await foreach (var reservation in _reservationRepository.RetrieveCollectionAsync(filter))
            {
                if (!reservation.IsCancelled)
                {
                    count++;
                    if (count > 0) // Workplace is already reserved (non-cancelled) for this date
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public async Task<bool> IsQuickReservationPossibleAsync(int userId, int workplaceId)
        {
            var nextWorkingDay = ReservationValidationHelper.GetNextWorkingDay();
            return await CanUserReserveForDateAsync(userId, nextWorkingDay) && 
                   await IsWorkplaceAvailableForDateAsync(workplaceId, nextWorkingDay);
        }

        
        private static ReservationResponseDto MapToResponseDto(Models.Reservation reservation)
        {
            return new ReservationResponseDto
            {
                ReservationId = reservation.ReservationId,
                UserId = reservation.UserId,
                WorkplaceId = reservation.WorkplaceId,
                ReservationDate = reservation.BookingDate,
                IsCancelled = reservation.IsCancelled,
                IsQuickReservation = false,
                Workplace = null 
            };
        }
    }
}
