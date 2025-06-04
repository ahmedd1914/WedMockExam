namespace WedMockExam.Services.DTOs.Workplace
{
    public class WorkplaceFilterRequestDto
    {
        public int? Floor { get; set; }
        public string? Zone { get; set; }
        public bool HasMonitor { get; set; }
        public bool HasDocking { get; set; }
        public bool HasWindow { get; set; }
        public bool HasPrinter { get; set; }
    }
}
