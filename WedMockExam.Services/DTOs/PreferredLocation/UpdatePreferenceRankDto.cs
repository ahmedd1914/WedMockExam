namespace WedMockExam.Services.DTOs.PreferredLocation
    {
    public class UpdatePreferenceRankDto
    {
        public int UserId { get; set; }
        public int WorkplaceId { get; set; }
        public int NewRank { get; set; }
    }
} 