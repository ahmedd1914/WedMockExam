using WedMockExam.Services.DTOs.Workplace;

namespace WedMockExam.Services.DTOs.Reservation
{
    public class ReservationResponseDto
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int WorkplaceId { get; set; }
        public DateTime ReservationDate { get; set; }
        public bool IsQuickReservation { get; set; }
        public bool IsCancelled { get; set; }
        public WorkplaceResponseDto Workplace { get; set; }
    }
} 