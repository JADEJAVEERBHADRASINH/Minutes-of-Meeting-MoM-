using System;
using System.ComponentModel.DataAnnotations;

namespace MOM.Models
{
    public class DepartmentModel
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}