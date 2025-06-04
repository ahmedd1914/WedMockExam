using WedMockExam.Repository.Base;

namespace WedMockExam.Repository.Interfaces.Reservation
{
    public interface IReservationRepository : IBaseRepository<Models.Reservation, ReservationFilter, ReservationUpdate>
    {
        
    }
}
    