namespace WedMockExam.Services.DTOs.Reservation
{
    public class ReservationRequestDto
    {
        public int UserId { get; set; }
        public int WorkplaceId { get; set; }
        public DateTime ReservationDate { get; set; }
    }
} 