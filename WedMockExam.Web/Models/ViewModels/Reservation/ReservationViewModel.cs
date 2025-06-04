using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WedMockExam.Services.DTOs.Reservation;
using WedMockExam.Services.DTOs.Workplace;

namespace WedMockExam.Web.Models.ViewModels.Reservation
{
    public class ReservationViewModel
    {
        // Reservation Details
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Workplace ID is required")]
        [Display(Name = "Workplace")]
        public int WorkplaceId { get; set; }

        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Reservation Date")]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        [Display(Name = "Is Cancelled")]
        public bool IsCancelled { get; set; }

        // Quick Reservation
        public bool IsQuickReservation { get; set; }

        // Lists and Collections
        public IEnumerable<ReservationResponseDto> UserReservations { get; set; } = new List<ReservationResponseDto>();
        public IEnumerable<ReservationResponseDto> DateReservations { get; set; } = new List<ReservationResponseDto>();
        public IEnumerable<WorkplaceResponseDto> AvailableWorkplaces { get; set; } = new List<WorkplaceResponseDto>();


        public DateTime SelectedDate { get; set; }
    }
}
