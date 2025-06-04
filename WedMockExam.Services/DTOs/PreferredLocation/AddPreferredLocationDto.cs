namespace WedMockExam.Services.DTOs.PreferredLocation
{
    public class AddPreferredLocationDto
    {
        public int UserId { get; set; }
        public int WorkplaceId { get; set; }
        public int PreferenceRank { get; set; }
    }
} 