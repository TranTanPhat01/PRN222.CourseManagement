using System.ComponentModel.DataAnnotations;
using CourseManagement.Models;

namespace PRN222.CourseManagement.Web.ViewModels
{
    /// <summary>
    /// ViewModel for Enrollment operations
    /// </summary>
    public class EnrollmentViewModel
    {
        [Required(ErrorMessage = "Student is required")]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Course is required")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Enrollment Date is required")]
        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        public DateTime EnrollDate { get; set; } = DateTime.Today;

        [Display(Name = "Grade")]
        [Range(0, 10, ErrorMessage = "Grade must be between 0 and 10")]
        public decimal? Grade { get; set; }

        [Display(Name = "Grade Finalized")]
        public bool IsGradeFinalized { get; set; }

        // Navigation properties for display
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
