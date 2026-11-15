
using System.ComponentModel.DataAnnotations;

namespace MOM.Models
{
    public class MeetingMemberModel
    {
        public int MeetingMemberID { get; set; }

        [Required(ErrorMessage = "Please select meeting")]
        public int MeetingID { get; set; }

        [Required(ErrorMessage = "Please select staff")]
        public int StaffID { get; set; }

        public bool IsPresent { get; set; }

        [Required(ErrorMessage = "Remarks is required")]
        public string? Remarks { get; set; }

        public string? StaffName { get; set; }

        public DateTime MeetingDate { get; set; }
    }
}