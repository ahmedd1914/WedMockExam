namespace WedMockExam.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int WorkplaceId { get; set; }
        public DateTime BookingDate { get; set; }
        public bool IsCancelled { get; set; }

    }
}