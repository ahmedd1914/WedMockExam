using WedMockExam.Services.DTOs.Workplace;

namespace WedMockExam.Services.DTOs.PreferredLocation
{
    public class PreferredLocationResponseDto
    {
        public int UserId { get; set; }
        public int WorkplaceId { get; set; }
        public int PreferenceRank { get; set; }
        public WorkplaceResponseDto Workplace { get; set; }
    }
} 