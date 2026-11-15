using System;
using System.ComponentModel.DataAnnotations;

namespace MOM.Models
{
    public class StaffModel
    {
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Staff Name is required")]
        public string StaffName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string MobileNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        public string EmailAddress { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        public int DepartmentID { get; set; } 

        public string? DepartmentName { get; set; }
    }
}