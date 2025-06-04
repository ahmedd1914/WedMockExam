using WedMockExam.Services.DTOs.Workplace;

namespace WedMockExam.Services.Interfaces.Workplace
{
    public interface IWorkplaceService
    {
        // Basic CRUD operations
        Task<WorkplaceResponseDto> GetWorkplaceByIdAsync(int workplaceId);
        Task<IEnumerable<WorkplaceResponseDto>> GetAllWorkplacesAsync();
        Task<WorkplaceResponseDto> CreateWorkplaceAsync(WorkplaceRequestDto request);
        Task<WorkplaceResponseDto> UpdateWorkplaceAsync(int workplaceId, WorkplaceRequestDto request);
        Task<bool> DeleteWorkplaceAsync(int workplaceId);

        // Search and filter operations
        Task<IEnumerable<WorkplaceResponseDto>> SearchWorkplacesAsync(WorkplaceFilterRequestDto filter);

        // Availability check operations
        Task<bool> IsWorkplaceAvailableAsync(int workplaceId, DateTime date);
        Task<IEnumerable<WorkplaceResponseDto>> GetAvailableWorkplacesForDateAsync(DateTime date);
        Task<IEnumerable<WorkplaceResponseDto>> GetAvailableWorkplacesAsync();
    }
} 