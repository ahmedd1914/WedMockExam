using WedMockExam.Repository.Interfaces.PreferredLocation;
using WedMockExam.Services.DTOs.PreferredLocation;
using WedMockExam.Services.Interfaces.PreferredLocation;
using WedMockExam.Services.Interfaces.Workplace;

namespace WedMockExam.Services.Implementations.PreferredLocation
{
    public class PreferredLocationService : IPreferredLocationService
    {
        private readonly IPreferredLocationRepository _preferredLocationRepository;
        private readonly IWorkplaceService _workplaceService;
        private const int MaxPreferredLocations = 3;

        public PreferredLocationService(
            IPreferredLocationRepository preferredLocationRepository,
            IWorkplaceService workplaceService)
        {
            _preferredLocationRepository = preferredLocationRepository;
            _workplaceService = workplaceService;
        }

        public async Task<bool> AddPreferredWorkplaceAsync(AddPreferredLocationDto request)
        {
            // Check if the workplace exists
            var workplace = await _workplaceService.GetWorkplaceByIdAsync(request.WorkplaceId);
            if (workplace == null)
            {
                throw new InvalidOperationException("The specified workplace does not exist.");
            }

            // Check if the user already has this workplace as a preferred location
            var existingLocations = await GetUserPreferredWorkplacesAsync(request.UserId);
            if (existingLocations.Any(l => l.WorkplaceId == request.WorkplaceId))
            {
                throw new InvalidOperationException("This workplace is already in your preferred locations.");
            }

            // Check if user has reached the maximum number of preferred locations
            if (!await CanAddMorePreferredLocationsAsync(request.UserId))
            {
                throw new InvalidOperationException($"Unable to add preferred location. You may have reached the maximum limit of {MaxPreferredLocations} locations.");
            }

            var preferredLocation = new Models.PreferredLocation
            {
                UserId = request.UserId,
                WorkplaceId = request.WorkplaceId,
                PreferenceRank = request.PreferenceRank
            };

            var id = await _preferredLocationRepository.CreateAsync(preferredLocation);
            return id > 0;
        }

        public async Task<bool> CanAddMorePreferredLocationsAsync(int userId)
        {
            var filter = new PreferredLocationFilter { UserId = userId };
            var count = 0;
            
            await foreach (var _ in _preferredLocationRepository.RetrieveCollectionAsync(filter))
            {
                count++;
                if (count >= MaxPreferredLocations)
                {
                    return false;
                }
            }
            
            return true;
        }

        public async Task<IEnumerable<PreferredLocationResponseDto>> GetAllPreferredLocationsAsync()
        {
            var locations = new List<PreferredLocationResponseDto>();
            await foreach (var location in _preferredLocationRepository.RetrieveCollectionAsync(new PreferredLocationFilter()))
            {
                locations.Add(await MapToResponseDto(location));
            }
            return locations;
        }
        public async Task<IEnumerable<PreferredLocationResponseDto>> GetUserPreferredWorkplacesAsync(int userId)
        {
            var filter = new PreferredLocationFilter { UserId = userId };
            var locations = new List<PreferredLocationResponseDto>();
            
            await foreach (var location in _preferredLocationRepository.RetrieveCollectionAsync(filter))
            {
                locations.Add(await MapToResponseDto(location));
            }
            
            return locations.OrderBy(l => l.PreferenceRank);
        }

        public async Task<IEnumerable<PreferredLocationResponseDto>> GetUserPreferredWorkplacesAvailableNextDayAsync(int userId)
        {
            DateTime nextDay = DateTime.Today.AddDays(1);
            while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1);
            }

            var preferred = await GetUserPreferredWorkplacesAsync(userId);

            var available = new List<PreferredLocationResponseDto>();
            foreach (var location in preferred)
            {
                bool isAvailable = await _workplaceService.IsWorkplaceAvailableAsync(location.WorkplaceId, nextDay);
                if (isAvailable)
                {
                    available.Add(location);
                }
            }

            return available;
        }

        public async Task<bool> RemovePreferredWorkplaceAsync(RemovePreferredLocationDto request)
        {
            var filter = new PreferredLocationFilter 
            { 
                UserId = request.UserId,
                WorkplaceId = request.WorkplaceId
            };

            var locations = new List<Models.PreferredLocation>();
            await foreach (var location in _preferredLocationRepository.RetrieveCollectionAsync(filter))
            {
                locations.Add(location);
            }

            if (!locations.Any())
            {
                throw new InvalidOperationException("The specified preferred location does not exist.");
            }

            var preferredLocation = locations.First();
            return await _preferredLocationRepository.DeletePreferredLocationAsync(preferredLocation.UserId, preferredLocation.WorkplaceId);
        }

        public async Task<bool> UpdatePreferenceRankAsync(UpdatePreferenceRankDto request)
        {
            if (request.NewRank < 1 || request.NewRank > MaxPreferredLocations)
            {
                throw new InvalidOperationException($"Preference rank must be between 1 and {MaxPreferredLocations}.");
            }

            // Get all locations for the user, ordered by current rank
            var filter = new PreferredLocationFilter { UserId = request.UserId };
            var locations = new List<Models.PreferredLocation>();
            await foreach (var location in _preferredLocationRepository.RetrieveCollectionAsync(filter))
            {
                locations.Add(location);
            }

            var targetLocation = locations.FirstOrDefault(l => l.WorkplaceId == request.WorkplaceId);
            if (targetLocation == null)
            {
                throw new InvalidOperationException("The specified preferred location does not exist.");
            }

            // Remove the target location from the list
            locations.Remove(targetLocation);

            // Insert the target location at the desired position (NewRank - 1)
            int insertIndex = Math.Min(request.NewRank - 1, locations.Count);
            locations.Insert(insertIndex, targetLocation);

            // Resequence all ranks
            for (int i = 0; i < locations.Count; i++)
            {
                var loc = locations[i];
                int newRank = i + 1;
                if (loc.PreferenceRank != newRank)
                {
                    var update = new PreferredLocationUpdate
                    {
                        WorkplaceId = loc.WorkplaceId,
                        PreferenceRank = newRank
                    };
                    await _preferredLocationRepository.UpdateAsync(loc.UserId, update);
                }
            }

            return true;
        }

        private async Task<PreferredLocationResponseDto> MapToResponseDto(Models.PreferredLocation location)
        {
            var workplace = await _workplaceService.GetWorkplaceByIdAsync(location.WorkplaceId);
            if (workplace == null)
            {
                throw new InvalidOperationException($"Workplace with ID {location.WorkplaceId} not found.");
            }

            return new PreferredLocationResponseDto
            {
                UserId = location.UserId,
                WorkplaceId = location.WorkplaceId,
                PreferenceRank = location.PreferenceRank,
                Workplace = workplace
            };
        }
    }
}
