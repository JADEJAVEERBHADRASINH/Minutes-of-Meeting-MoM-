using System;
using System.ComponentModel.DataAnnotations;

namespace MOM.Models
{
    public class MeetingVenueModel
    {
        public int MeetingVenueID { get; set; }

        [Required(ErrorMessage = "Meeting Venue Name is required")]
        [StringLength(100)]
        public string MeetingVenueName { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
