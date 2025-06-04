using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WedMockExam.Services.DTOs.Workplace;
using WedMockExam.Services.DTOs.Reservation;
using WedMockExam.Services.DTOs.PreferredLocation;

namespace WedMockExam.Web.Models.ViewModels.Home
{
    public class HomeViewModel
    {
        public IEnumerable<WorkplaceResponseDto> PreferredWorkplaces { get; set; } = new List<WorkplaceResponseDto>();
        // User's current reservations
        public IEnumerable<ReservationResponseDto> UserReservations { get; set; } = new List<ReservationResponseDto>();

        // Selected date for viewing/bookings
        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        [Display(Name = "Selected Date")]
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        // Preferred Locations
        public bool CanAddMorePreferredLocations { get; set; }
        // Standard Reservation
        public ReservationRequestDto NewReservation { get; set; } = new ReservationRequestDto();

        // Search Filters
        public WorkplaceFilterRequestDto WorkplaceFilters { get; set; } = new WorkplaceFilterRequestDto();
    }
}
       
    

