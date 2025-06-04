using WedMockExam.Services.DTOs.Workplace;
using WedMockExam.Services.DTOs.PreferredLocation;

namespace WedMockExam.Services.Interfaces.PreferredLocation
{
    public interface IPreferredLocationService
    {
        // Get user's preferred workplaces
        Task<IEnumerable<PreferredLocationResponseDto>> GetUserPreferredWorkplacesAsync(int userId);
        // Get all preferred locations (admin)
        Task<IEnumerable<PreferredLocationResponseDto>> GetAllPreferredLocationsAsync();
        Task<IEnumerable<PreferredLocationResponseDto>> GetUserPreferredWorkplacesAvailableNextDayAsync(int userId);


        // Add a workplace to user's preferred locations
        // Returns false if user already has 3 preferred locations
        Task<bool> AddPreferredWorkplaceAsync(AddPreferredLocationDto request);

        // Remove a workplace from user's preferred locations
        Task<bool> RemovePreferredWorkplaceAsync(RemovePreferredLocationDto request);

        // Update the rank of a preferred workplace
        Task<bool> UpdatePreferenceRankAsync(UpdatePreferenceRankDto request);

        // Check if user can add more preferred locations
        Task<bool> CanAddMorePreferredLocationsAsync(int userId);
    }
} 