using System;
using System.ComponentModel.DataAnnotations;

namespace MOM.Models
{
    public class MeetingTypeModel
    {
        public int MeetingTypeID { get; set; }

        [Required(ErrorMessage = "Meeting Type Name is required")]
        [StringLength(100)]
        public string MeetingTypeName { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
