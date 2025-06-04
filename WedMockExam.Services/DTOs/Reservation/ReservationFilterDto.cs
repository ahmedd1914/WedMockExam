namespace WedMockExam.Services.DTOs.Reservation
{
    public class ReservationFilterDto
    {
        public int? UserId { get; set; }
        public int? WorkplaceId { get; set; }
        public DateTime? BookingDate { get; set; }
    }
} 