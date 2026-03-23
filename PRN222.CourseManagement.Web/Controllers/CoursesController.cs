using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CourseManagement.Models;
using CourseManagement.Services.Interfaces;
using PRN222.CourseManagement.Web.ViewModels;

namespace PRN222.CourseManagement.Web.Controllers
{
    /// <summary>
    /// Courses Controller - Manages course CRUD operations
    /// STRICT RULE: Only calls ICourseService and IDepartmentService
    /// NO direct access to DbContext or Repository
    /// </summary>
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IDepartmentService _departmentService;

        public CoursesController(
            ICourseService courseService,
            IDepartmentService departmentService)
        {
            _courseService = courseService;
            _departmentService = departmentService;
        }

        // GET: Coursesa
        public IActionResult Index()
        {
            var result = _courseService.GetAllCourses();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<Course>());
            }

            return View(result.Data);
        }

        // GET: Courses/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Course ID is required";
                return RedirectToAction(nameof(Index));
            }

            var result = _courseService.GetCourseById(id.Value);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            LoadDepartmentsDropdown();
            LoadCourseStatusDropdown();
            return View(new CourseViewModel());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDepartmentsDropdown();
                LoadCourseStatusDropdown();
                return View(model);
            }

            // Map ViewModel to Domain Model
            var course = new Course
            {
                CourseCode = model.CourseCode,
                Title = model.Title,
                Credits = model.Credits,
                DepartmentId = model.DepartmentId,
                Status = model.Status
            };

            var result = _courseService.AddCourse(course);

            if (!result.IsSuccess)
            {
                // Translate ServiceResult errors to ModelState
                ModelState.AddModelError(string.Empty, result.Message);
                LoadDepartmentsDropdown();
                LoadCourseStatusDropdown();
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Course ID is required";
                return RedirectToAction(nameof(Index));
            }

            var result = _courseService.GetCourseById(id.Value);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            // Map Domain Model to ViewModel
            if (result.Data == null)
            {
                TempData["ErrorMessage"] = "Course data is empty";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new CourseViewModel
            {
                CourseId = result.Data.CourseId,
                CourseCode = result.Data.CourseCode,
                Title = result.Data.Title,
                Credits = result.Data.Credits,
                DepartmentId = result.Data.DepartmentId,
                Status = result.Data.Status
            };

            LoadDepartmentsDropdown();
            LoadCourseStatusDropdown();
            return View(viewModel);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CourseViewModel model)
        {
            if (id != model.CourseId)
            {
                TempData["ErrorMessage"] = "Course ID mismatch";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                LoadDepartmentsDropdown();
                LoadCourseStatusDropdown();
                return View(model);
            }

            // Map ViewModel to Domain Model
            var course = new Course
            {
                CourseId = model.CourseId,
                CourseCode = model.CourseCode,
                Title = model.Title,
                Credits = model.Credits,
                DepartmentId = model.DepartmentId,
                Status = model.Status
            };

            var result = _courseService.UpdateCourse(course);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                LoadDepartmentsDropdown();
                LoadCourseStatusDropdown();
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Course ID is required";
                return RedirectToAction(nameof(Index));
            }

            var result = _courseService.GetCourseById(id.Value);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var result = _courseService.DeleteCourse(id);

            if (!result.IsSuccess)
            {
                // Business rule violation (e.g., course has enrollments)
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to load departments for dropdown
        /// </summary>
        private void LoadDepartmentsDropdown()
        {
            var departmentsResult = _departmentService.GetAllDepartments();

            if (departmentsResult.IsSuccess)
            {
                ViewBag.Departments = new SelectList(
                    departmentsResult.Data,
                    "DepartmentId",
                    "Name"
                );
            }
            else
            {
                ViewBag.Departments = new SelectList(Enumerable.Empty<Department>());
                TempData["WarningMessage"] = "Could not load departments";
            }
        }

        /// <summary>
        /// Helper method to load course status for dropdown
        /// </summary>
        private void LoadCourseStatusDropdown()
        {
            ViewBag.CourseStatuses = new SelectList(
                Enum.GetValues(typeof(CourseStatus))
                    .Cast<CourseStatus>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value",
                "Text"
            );
        }
    }
}
