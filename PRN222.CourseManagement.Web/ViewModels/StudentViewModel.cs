using System.ComponentModel.DataAnnotations;
using CourseManagement.Models;

namespace PRN222.CourseManagement.Web.ViewModels
{
    /// <summary>
    /// ViewModel for Student Create/Edit operations
    /// </summary>
    public class StudentViewModel
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student Code is required")]
        [StringLength(20, ErrorMessage = "Student Code cannot exceed 20 characters")]
        [Display(Name = "Student Code")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        // Navigation properties for display
        public Department? Department { get; set; }
        public IEnumerable<Enrollment>? Enrollments { get; set; }
    }
}
