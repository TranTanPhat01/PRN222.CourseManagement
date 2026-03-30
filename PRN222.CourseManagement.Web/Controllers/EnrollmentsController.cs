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
        private const string ErrorMessageKey = "ErrorMessage";
        private const string SuccessMessageKey = "SuccessMessage";
        private const string WarningMessageKey = "WarningMessage";

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
                TempData[ErrorMessageKey] = result.Message;
                return View(new List<Enrollment>());
            }

            return View(result.Data);
        }

        // GET: Enrollments/Details?studentId=1&courseId=1
        [HttpGet]
        public IActionResult Details(int? studentId, int? courseId)
        {
            if (studentId == null || courseId == null || !ModelState.IsValid)
            {
                TempData[ErrorMessageKey] = "Both Student ID and Course ID are required";
                return RedirectToAction(nameof(Index));
            }

            var result = _enrollmentService.GetEnrollment(studentId.Value, courseId.Value);

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
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

            var result = _enrollmentService.EnrollStudent(
                model.StudentId ?? 0,
                model.CourseId ?? 0,
                model.EnrollDate ?? DateTime.Today
            );

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                LoadStudentsDropdown();
                LoadCoursesDropdown();
                return View(model);
            }

            TempData[SuccessMessageKey] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/AssignGrade?studentId=1&courseId=1
        [HttpGet]
        public IActionResult AssignGrade(int? studentId, int? courseId)
        {
            if (studentId == null || courseId == null || !ModelState.IsValid)
            {
                TempData[ErrorMessageKey] = "Both Student ID and Course ID are required";
                return RedirectToAction(nameof(Index));
            }

            var result = _enrollmentService.GetEnrollment(studentId.Value, courseId.Value);

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            if (result.Data == null)
            {
                TempData[ErrorMessageKey] = "Enrollment data is empty";
                return RedirectToAction(nameof(Index));
            }

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
                    model.StudentId ?? 0,
                    model.CourseId ?? 0,
                    model.Grade.Value
                );

                if (result.IsSuccess)
                {
                    TempData[SuccessMessageKey] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }
            else
            {
                ModelState.AddModelError("Grade", "Grade is required");
            }

            // Reload Student and Course for the view if we reach here (error cases)
            var enrollmentResult = _enrollmentService.GetEnrollment(
                model.StudentId ?? 0,
                model.CourseId ?? 0);

            if (enrollmentResult.IsSuccess && enrollmentResult.Data != null)
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
                TempData[ErrorMessageKey] = result.Message;
            }
            else
            {
                TempData[SuccessMessageKey] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/Delete?studentId=1&courseId=1 - shows unenroll confirmation
        [HttpGet]
        public IActionResult Delete(int? studentId, int? courseId)
        {
            if (studentId == null || courseId == null || !ModelState.IsValid)
            {
                TempData[ErrorMessageKey] = "Both Student ID and Course ID are required";
                return RedirectToAction(nameof(Index));
            }

            var result = _enrollmentService.GetEnrollment(studentId.Value, courseId.Value);

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            // Flag so the Delete view shows unenroll warning instead of details
            ViewBag.IsDeleteConfirm = true;
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
                TempData[ErrorMessageKey] = result.Message;
            }
            else
            {
                TempData[SuccessMessageKey] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Enrollments/ByStudent/5
        public IActionResult ByStudent(int? id)
        {
            if (id == null)
            {
                TempData[ErrorMessageKey] = "Student ID is required";
                return RedirectToAction(nameof(Index));
            }

            var studentResult = _studentService.GetStudentById(id.Value);
            if (!studentResult.IsSuccess)
            {
                TempData[ErrorMessageKey] = studentResult.Message;
                return RedirectToAction(nameof(Index));
            }

            var enrollmentsResult = _enrollmentService.GetEnrollmentsByStudent(id.Value);

            if (!enrollmentsResult.IsSuccess)
            {
                TempData[ErrorMessageKey] = enrollmentsResult.Message;
                return View(new List<Enrollment>());
            }

            ViewBag.StudentName = studentResult.Data!.FullName;
            return View(enrollmentsResult.Data);
        }

        // GET: Enrollments/ByCourse/5
        public IActionResult ByCourse(int? id)
        {
            if (id == null)
            {
                TempData[ErrorMessageKey] = "Course ID is required";
                return RedirectToAction(nameof(Index));
            }

            var courseResult = _courseService.GetCourseById(id.Value);
            if (!courseResult.IsSuccess)
            {
                TempData[ErrorMessageKey] = courseResult.Message;
                return RedirectToAction(nameof(Index));
            }

            var enrollmentsResult = _enrollmentService.GetEnrollmentsByCourse(id.Value);

            if (!enrollmentsResult.IsSuccess)
            {
                TempData[ErrorMessageKey] = enrollmentsResult.Message;
                return View(new List<Enrollment>());
            }

            ViewBag.CourseTitle = courseResult.Data!.Title;
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
                    studentsResult.Data!.Where(s => s.IsActive).ToList(),
                    "StudentId",
                    "FullName"
                );
            }
            else
            {
                ViewBag.Students = new SelectList(Enumerable.Empty<Student>());
                TempData[WarningMessageKey] = "Could not load students";
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
                    coursesResult.Data!.Where(c => c.Status == CourseStatus.Active).ToList(),
                    "CourseId",
                    "Title"
                );
            }
            else
            {
                ViewBag.Courses = new SelectList(Enumerable.Empty<Course>());
                TempData[WarningMessageKey] = "Could not load courses";
            }
        }
    }
}
