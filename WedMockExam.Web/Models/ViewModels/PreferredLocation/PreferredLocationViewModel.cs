using System.ComponentModel.DataAnnotations;
using WedMockExam.Services.DTOs.PreferredLocation;
using WedMockExam.Services.DTOs.Workplace;

namespace WedMockExam.Web.Models.ViewModels.PreferredLocation
{
    public class PreferredLocationViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please select a workplace")]
        [Display(Name = "Workplace")]
        public int WorkplaceId { get; set; }

        [Required(ErrorMessage = "Please specify a preference rank")]
        [Range(1, 3, ErrorMessage = "Preference rank must be between 1 and 3")]
        [Display(Name = "Preference Rank")]
        public int PreferenceRank { get; set; }

        public IEnumerable<PreferredLocationResponseDto> UserPreferredLocations { get; set; } = new List<PreferredLocationResponseDto>();
        public IEnumerable<WorkplaceResponseDto> AvailableWorkplaces { get; set; } = new List<WorkplaceResponseDto>();

    }
}