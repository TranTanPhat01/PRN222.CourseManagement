using System.ComponentModel.DataAnnotations;
using CourseManagement.Models;

namespace PRN222.CourseManagement.Web.ViewModels
{
    /// <summary>
    /// ViewModel for Course Create/Edit operations
    /// </summary>
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course Code is required")]
        [StringLength(20, ErrorMessage = "Course Code cannot exceed 20 characters")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Course Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Credits is required")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Display(Name = "Status")]
        public CourseStatus Status { get; set; } = CourseStatus.Active;

        // Navigation properties for display
        public Department? Department { get; set; }
        public IEnumerable<Enrollment>? Enrollments { get; set; }
    }
}
