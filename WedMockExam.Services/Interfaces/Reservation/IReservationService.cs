using WedMockExam.Services.DTOs.Reservation;

namespace WedMockExam.Services.Interfaces.Reservation
{
    public interface IReservationService
    {
        // Standard reservation operations
        Task<ReservationResponseDto> CreateReservationAsync(ReservationRequestDto request);
        Task<ReservationResponseDto> GetReservationByIdAsync(int reservationId);
        Task<IEnumerable<ReservationResponseDto>> GetUserReservationsAsync(int userId);
        Task<bool> CancelReservationAsync(int reservationId);

        // Quick reservation operations
        Task<ReservationResponseDto> CreateQuickReservationAsync(QuickReservationRequestDto request);

        // Validation operations
        Task<bool> CanUserReserveForDateAsync(int userId, DateTime date);
        Task<bool> IsWorkplaceAvailableForDateAsync(int workplaceId, DateTime date);
        Task<bool> IsQuickReservationPossibleAsync(int userId, int workplaceId);

        // Additional operations
        Task<IEnumerable<ReservationResponseDto>> GetReservationsForDateAsync(DateTime date);

        // Search operations
        Task<IEnumerable<ReservationResponseDto>> SearchReservationsAsync(ReservationFilterDto filter);
    
    }
} 