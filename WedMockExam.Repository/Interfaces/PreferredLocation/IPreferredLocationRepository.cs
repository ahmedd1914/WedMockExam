using WedMockExam.Repository.Base;

namespace WedMockExam.Repository.Interfaces.PreferredLocation
{
    public interface IPreferredLocationRepository : IBaseRepository<Models.PreferredLocation, PreferredLocationFilter, PreferredLocationUpdate>
    {
        Task<bool> DeletePreferredLocationAsync(int userId, int workplaceId);
    }
}