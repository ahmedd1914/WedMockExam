using WedMockExam.Repository.Interfaces.Workplace;
using WedMockExam.Services.DTOs.Workplace;
using WedMockExam.Services.Interfaces.Workplace;
using WedMockExam.Services.Interfaces.Reservation;
using Microsoft.Extensions.Logging;

namespace WedMockExam.Services.Implementations.Workplace
{
    public class WorkplaceService : IWorkplaceService
    {
        private readonly IWorkplaceRepository _workplaceRepository;
        private readonly IReservationService _reservationService;
        private readonly ILogger<WorkplaceService> _logger;

        public WorkplaceService(
            IWorkplaceRepository workplaceRepository, 
            IReservationService reservationService,
            ILogger<WorkplaceService> logger)
        {
            _workplaceRepository = workplaceRepository;
            _reservationService = reservationService;
            _logger = logger;
        }

        public async Task<WorkplaceResponseDto> CreateWorkplaceAsync(WorkplaceRequestDto request)
        {
            var workplace = new Models.Workplace
            {
                Floor = request.Floor,
                Zone = request.Zone,
                HasMonitor = request.HasMonitor,
                HasDocking = request.HasDocking,
                HasWindow = request.HasWindow,
                HasPrinter = request.HasPrinter
            };

            var id = await _workplaceRepository.CreateAsync(workplace);
            return await GetWorkplaceByIdAsync(id);
        }

        public async Task<bool> DeleteWorkplaceAsync(int workplaceId)
        {
            return await _workplaceRepository.DeleteAsync(workplaceId);
        }

        public async Task<IEnumerable<WorkplaceResponseDto>> GetAllWorkplacesAsync()
        {
            var workplaces = new List<WorkplaceResponseDto>();
            await foreach (var workplace in _workplaceRepository.RetrieveCollectionAsync(new WorkplaceFilter()))
            {
                workplaces.Add(MapToResponseDto(workplace));
            }
            return workplaces;
        }

        public async Task<IEnumerable<WorkplaceResponseDto>> GetAvailableWorkplacesForDateAsync(DateTime date)
        {
            var allWorkplaces = await GetAllWorkplacesAsync();
            var availableWorkplaces = new List<WorkplaceResponseDto>();

            foreach (var workplace in allWorkplaces)
            {
                if (await _reservationService.IsWorkplaceAvailableForDateAsync(workplace.WorkplaceId, date))
                {
                    availableWorkplaces.Add(workplace);
                }
            }

            return availableWorkplaces;
        }
        public async Task<IEnumerable<WorkplaceResponseDto>> GetAvailableWorkplacesAsync()
        {
            return await GetAvailableWorkplacesForDateAsync(DateTime.Today);
        }

        public async Task<WorkplaceResponseDto> GetWorkplaceByIdAsync(int workplaceId)
        {
            var workplace = await _workplaceRepository.RetrieveAsync(workplaceId);
            return workplace != null ? MapToResponseDto(workplace) : null;
        }

        public async Task<bool> IsWorkplaceAvailableAsync(int workplaceId, DateTime date)
        {
            return await _reservationService.IsWorkplaceAvailableForDateAsync(workplaceId, date);
        }

        public async Task<IEnumerable<WorkplaceResponseDto>> SearchWorkplacesAsync(WorkplaceFilterRequestDto filter)
        {
            _logger.LogInformation("Starting workplace search with filters: Floor={Floor}, Zone={Zone}, HasMonitor={HasMonitor}, HasDocking={HasDocking}, HasWindow={HasWindow}, HasPrinter={HasPrinter}",
                filter.Floor, filter.Zone, filter.HasMonitor, filter.HasDocking, filter.HasWindow, filter.HasPrinter);

            var workplaceFilter = new WorkplaceFilter();

            // Only set floor if it has a value
            if (filter.Floor.HasValue && filter.Floor.Value > 0)
            {
                workplaceFilter.Floor = filter.Floor.Value;
                _logger.LogInformation("Setting floor filter to: {Floor}", filter.Floor.Value);
            }

            // Only set zone if it has a value
            if (!string.IsNullOrWhiteSpace(filter.Zone))
            {
                workplaceFilter.Zone = filter.Zone;
                _logger.LogInformation("Setting zone filter to: {Zone}", filter.Zone);
            }

            // Only set boolean filters if they are explicitly checked (true)
            if (filter.HasMonitor)
            {
                workplaceFilter.HasMonitor = true;
                _logger.LogInformation("Setting HasMonitor filter to true");
            }
            if (filter.HasDocking)
            {
                workplaceFilter.HasDocking = true;
                _logger.LogInformation("Setting HasDocking filter to true");
            }
            if (filter.HasWindow)
            {
                workplaceFilter.HasWindow = true;
                _logger.LogInformation("Setting HasWindow filter to true");
            }
            if (filter.HasPrinter)
            {
                workplaceFilter.HasPrinter = true;
                _logger.LogInformation("Setting HasPrinter filter to true");
            }

            var workplaces = new List<WorkplaceResponseDto>();
            await foreach (var workplace in _workplaceRepository.RetrieveCollectionAsync(workplaceFilter))
            {
                workplaces.Add(MapToResponseDto(workplace));
            }

            _logger.LogInformation("Search completed. Found {Count} workplaces matching the criteria", workplaces.Count);
            return workplaces;
        }

        public async Task<WorkplaceResponseDto> UpdateWorkplaceAsync(int workplaceId, WorkplaceRequestDto request)
        {
            var update = new WorkplaceUpdate
            {
                Floor = request.Floor,
                Zone = request.Zone,
                HasMonitor = request.HasMonitor,
                HasDocking = request.HasDocking,
                HasWindow = request.HasWindow,
                HasPrinter = request.HasPrinter
            };

            var success = await _workplaceRepository.UpdateAsync(workplaceId, update);
            return success ? await GetWorkplaceByIdAsync(workplaceId) : null;
        }

        private static WorkplaceResponseDto MapToResponseDto(Models.Workplace workplace)
        {
            return new WorkplaceResponseDto
            {
                WorkplaceId = workplace.WorkplaceId,
                Floor = workplace.Floor,
                Zone = workplace.Zone,
                HasMonitor = workplace.HasMonitor,
                HasDocking = workplace.HasDocking,
                HasWindow = workplace.HasWindow,
                HasPrinter = workplace.HasPrinter
            };
        }
    }
}
    