using System.Data.SqlTypes;

namespace WedMockExam.Repository.Interfaces.Reservation
{
    public class ReservationFilter
    {
        public SqlInt32? UserId { get; set; }
        public SqlInt32? WorkplaceId { get; set; }
        public SqlDateTime? BookingDate { get; set; }

}
}