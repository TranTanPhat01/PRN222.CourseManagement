using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CourseManagement.Models;
using CourseManagement.Services.Interfaces;
using PRN222.CourseManagement.Web.ViewModels;

namespace PRN222.CourseManagement.Web.Controllers
{
    /// <summary>
    /// Enrollments Controller - Manages enrollment operations
    /// STRICT RULE: Only calls IEnrollmentService, IStudentService, ICourseService
    /// NO direct access to DbContext or Repository
    /// </summary>
    public class EnrollmentsController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;

        public EnrollmentsController(
            IEnrollmentService enrollmentService,
            IStudentService studentService,
            ICourseService courseService)
        {
            _enrollmentService = enrollmentService;
            _studentService = studentService;
            _courseService = courseService;
        }

        // GET: Enrollments
        public IActionResult Index()
        {
            var result = _enrollmentService.GetAllEnrollments();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<Enrollment>());
            }

            return View(result.Data);
        }

        // GET: Enrollments/Details?studentId=1&courseId=1
        public IActionResult Details(int? studentId, int? courseId)
        {
            if (studentId == null || courseId == null)
            {
                TempData["ErrorMessage"] = "Both Student ID and Course ID are required";
                return RedirectToAction(nameof(Index));
            }

            var result = _enrollmentService.GetEnrollment(studentId.Value, courseId.Value);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            LoadStudentsDropdown();
            LoadCoursesDropdown();
            return View(new EnrollmentViewModel());
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EnrollmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadStudentsDropdown();
                LoadCoursesDropdown();
                return View(model);
            }

            // Call service to enroll student
            var result = _enrollmentService.EnrollStudent(
                model.StudentId,
                model.CourseId,
                model.EnrollDate
            );

            if (!result.IsSuccess)
            {
                // Translate ServiceResult errors to ModelState
                // This could be duplicate enrollment, course inactive, etc.
                ModelState.AddModelError(string.Empty, result.Message);
                LoadStudentsDropdown();
                LoadCoursesDropdown();
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/AssignGrade?studentId=1&courseId=1
        public IActionResult AssignGrade(int? studentId, int? courseId)
        {
            if (studentId == null || courseId == null)
            {
                TempData["ErrorMessage"] = "Both Student ID and Course ID are required";
                return RedirectToAction(nameof(Index));
            }

            var result = _enrollmentService.GetEnrollment(studentId.Value, courseId.Value);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            // Map Domain Model to ViewModel
            var viewModel = new EnrollmentViewModel
            {
                StudentId = result.Data.StudentId,
                CourseId = result.Data.CourseId,
                EnrollDate = result.Data.EnrollDate,
                Grade = result.Data.Grade,
                IsGradeFinalized = result.Data.IsGradeFinalized,
                Student = result.Data.Student,
                Course = result.Data.Course
            };

            return View(viewModel);
        }

        // POST: Enrollments/AssignGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignGrade(EnrollmentViewModel model)
        {
            if (model.Grade.HasValue)
            {
                var result = _enrollmentService.AssignGrade(
                    model.StudentId,
                    model.CourseId,
                    model.Grade.Value
                );

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }
            else
            {
                ModelState.AddModelError("Grade", "Grade is required");
            }

            // Reload Student and Course for the view if we reach here (error cases)
            var enrollmentResult = _enrollmentService.GetEnrollment(model.StudentId, model.CourseId);
            if (enrollmentResult.IsSuccess)
            {
                model.Student = enrollmentResult.Data.Student;
                model.Course = enrollmentResult.Data.Course;
            }

            return View(model);
        }

        // POST: Enrollments/FinalizeGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizeGrade(int studentId, int courseId)
        {
            var result = _enrollmentService.FinalizeGrade(studentId, courseId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/Delete?studentId=1&courseId=1
        public IActionResult Delete(int? studentId, int? courseId)
        {
            if (studentId == null || courseId == null)
            {
                TempData["ErrorMessage"] = "Both Student ID and Course ID are required";
                return RedirectToAction(nameof(Index));
            }

            var result = _enrollmentService.GetEnrollment(studentId.Value, courseId.Value);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // POST: Enrollments/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int studentId, int courseId)
        {
            var result = _enrollmentService.UnenrollStudent(studentId, courseId);

            if (!result.IsSuccess)
            {
                // Business rule violation (e.g., grade is finalized)
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/ByStudent/5
        public IActionResult ByStudent(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Student ID is required";
                return RedirectToAction(nameof(Index));
            }

            var studentResult = _studentService.GetStudentById(id.Value);
            if (!studentResult.IsSuccess)
            {
                TempData["ErrorMessage"] = studentResult.Message;
                return RedirectToAction(nameof(Index));
            }

            var enrollmentsResult = _enrollmentService.GetEnrollmentsByStudent(id.Value);

            if (!enrollmentsResult.IsSuccess)
            {
                TempData["ErrorMessage"] = enrollmentsResult.Message;
                return View(new List<Enrollment>());
            }

            ViewBag.StudentName = studentResult.Data.FullName;
            return View(enrollmentsResult.Data);
        }

        // GET: Enrollments/ByCourse/5
        public IActionResult ByCourse(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Course ID is required";
                return RedirectToAction(nameof(Index));
            }

            var courseResult = _courseService.GetCourseById(id.Value);
            if (!courseResult.IsSuccess)
            {
                TempData["ErrorMessage"] = courseResult.Message;
                return RedirectToAction(nameof(Index));
            }

            var enrollmentsResult = _enrollmentService.GetEnrollmentsByCourse(id.Value);

            if (!enrollmentsResult.IsSuccess)
            {
                TempData["ErrorMessage"] = enrollmentsResult.Message;
                return View(new List<Enrollment>());
            }

            ViewBag.CourseTitle = courseResult.Data.Title;
            return View(enrollmentsResult.Data);
        }

        /// <summary>
        /// Helper method to load students for dropdown
        /// </summary>
        private void LoadStudentsDropdown()
        {
            var studentsResult = _studentService.GetAllStudents();

            if (studentsResult.IsSuccess)
            {
                ViewBag.Students = new SelectList(
                    studentsResult.Data.Where(s => s.IsActive).ToList(),
                    "StudentId",
                    "FullName"
                );
            }
            else
            {
                ViewBag.Students = new SelectList(Enumerable.Empty<Student>());
                TempData["WarningMessage"] = "Could not load students";
            }
        }

        /// <summary>
        /// Helper method to load active courses for dropdown
        /// </summary>
        private void LoadCoursesDropdown()
        {
            var coursesResult = _courseService.GetAllCourses();

            if (coursesResult.IsSuccess)
            {
                ViewBag.Courses = new SelectList(
                    coursesResult.Data.Where(c => c.Status == CourseStatus.Active).ToList(),
                    "CourseId",
                    "Title"
                );
            }
            else
            {
                ViewBag.Courses = new SelectList(Enumerable.Empty<Course>());
                TempData["WarningMessage"] = "Could not load courses";
            }
        }
    }
}
