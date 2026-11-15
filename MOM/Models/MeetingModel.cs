using System;
using System.ComponentModel.DataAnnotations;

namespace MOM.Models
{
    public class MeetingModel
    {
        public int MeetingID { get; set; }

        [Required(ErrorMessage = "Meeting Date is required")]
        public DateTime? MeetingDate { get; set; }

        [Required(ErrorMessage = "Meeting Venue is required")]
        public int MeetingVenueID { get; set; }

        [Required(ErrorMessage = "Meeting Type is required")]
        public int MeetingTypeID { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentID { get; set; }

        public string? MeetingVenueName { get; set; }

        public string? MeetingTypeName { get; set; }

        public string? DepartmentName { get; set; }

        [Required(ErrorMessage = "Meeting Description is required")]
        public string? MeetingDescription { get; set; }

        public string? DocumentPath { get; set; }

        public bool IsCancelled { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}