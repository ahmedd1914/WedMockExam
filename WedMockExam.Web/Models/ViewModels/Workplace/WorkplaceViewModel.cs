using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WedMockExam.Services.DTOs.Workplace;

namespace WedMockExam.Web.Models.ViewModels.Workplace
{
    public class WorkplaceViewModel
    {
        // Workplace Details
        public int WorkplaceId { get; set; }

        [Required(ErrorMessage = "Floor is required")]
        [Range(1, 10, ErrorMessage = "Floor must be between 1 and 10")]
        [Display(Name = "Floor")]
        public int Floor { get; set; }

        [Required(ErrorMessage = "Zone is required")]
        [StringLength(10, ErrorMessage = "Zone cannot exceed 10 characters")]
        [Display(Name = "Zone")]
        public string Zone { get; set; }

        [Display(Name = "Has Monitor")]
        public bool HasMonitor { get; set; }

        [Display(Name = "Has Docking Station")]
        public bool HasDocking { get; set; }

        [Display(Name = "Has Window")]
        public bool HasWindow { get; set; }

        [Display(Name = "Has Printer")]
        public bool HasPrinter { get; set; }

        // Lists and Collections
        public IEnumerable<WorkplaceResponseDto> AllWorkplaces { get; set; } = new List<WorkplaceResponseDto>();
        public IEnumerable<WorkplaceResponseDto> FilteredWorkplaces { get; set; } = new List<WorkplaceResponseDto>();


        // Search Filters
        public WorkplaceFilterRequestDto SearchFilters { get; set; } = new WorkplaceFilterRequestDto();

        // Date Selection for Availability
        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        [Display(Name = "Selected Date")]
        public DateTime SelectedDate { get; set; } = DateTime.Today;




        // Validation States
        public bool IsWorkplaceAvailable { get; set; }
    }
}
