using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CourseManagement.Models;
using CourseManagement.Services.Interfaces;
using PRN222.CourseManagement.Web.ViewModels;

namespace PRN222.CourseManagement.Web.Controllers
{
    /// <summary>
    /// Students Controller - Manages student CRUD operations
    /// STRICT RULE: Only calls IStudentService and IDepartmentService
    /// NO direct access to DbContext or Repository
    /// </summary>
    public class StudentsController : Controller
    {
        private const string ErrorMessageKey = "ErrorMessage";
        private const string SuccessMessageKey = "SuccessMessage";
        private const string WarningMessageKey = "WarningMessage";

        private readonly IStudentService _studentService;
        private readonly IDepartmentService _departmentService;

        public StudentsController(
            IStudentService studentService,
            IDepartmentService departmentService)
        {
            _studentService = studentService;
            _departmentService = departmentService;
        }

        // GET: Students
        public IActionResult Index()
        {
            var result = _studentService.GetAllStudents();

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
                return View(new List<Student>());
            }

            return View(result.Data);
        }

        // GET: Students/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData[ErrorMessageKey] = "Student ID is required";
                return RedirectToAction(nameof(Index));
            }

            var result = _studentService.GetStudentById(id.Value);

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            LoadDepartmentsDropdown();
            return View(new StudentViewModel());
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDepartmentsDropdown();
                return View(model);
            }

            // Map ViewModel to Domain Model
            var student = new Student
            {
                StudentCode = model.StudentCode,
                FullName = model.FullName,
                Email = model.Email,
                DepartmentId = model.DepartmentId ?? 0,
                DateOfBirth = model.DateOfBirth ?? DateTime.Today,
                IsActive = model.IsActive
            };

            var result = _studentService.AddStudent(student);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                LoadDepartmentsDropdown();
                return View(model);
            }

            TempData[SuccessMessageKey] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData[ErrorMessageKey] = "Student ID is required";
                return RedirectToAction(nameof(Index));
            }

            var result = _studentService.GetStudentById(id.Value);

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            if (result.Data == null)
            {
                TempData[ErrorMessageKey] = "Student data is empty";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new StudentViewModel
            {
                StudentId = result.Data.StudentId,
                StudentCode = result.Data.StudentCode,
                FullName = result.Data.FullName,
                Email = result.Data.Email,
                DepartmentId = result.Data.DepartmentId,
                DateOfBirth = result.Data.DateOfBirth,
                IsActive = result.Data.IsActive
            };

            LoadDepartmentsDropdown();
            return View(viewModel);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StudentViewModel model)
        {
            if (id != (model.StudentId ?? 0))
            {
                TempData[ErrorMessageKey] = "Student ID mismatch";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                LoadDepartmentsDropdown();
                return View(model);
            }

            var student = new Student
            {
                StudentId = id,
                StudentCode = model.StudentCode,
                FullName = model.FullName,
                Email = model.Email,
                DepartmentId = model.DepartmentId ?? 0,
                DateOfBirth = model.DateOfBirth ?? DateTime.Today,
                IsActive = model.IsActive
            };

            var result = _studentService.UpdateStudent(student);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                LoadDepartmentsDropdown();
                return View(model);
            }

            TempData[SuccessMessageKey] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData[ErrorMessageKey] = "Student ID is required";
                return RedirectToAction(nameof(Index));
            }

            var result = _studentService.GetStudentById(id.Value);

            if (!result.IsSuccess)
            {
                TempData[ErrorMessageKey] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var result = _studentService.DeleteStudent(id);

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
                TempData[WarningMessageKey] = "Could not load departments";
            }
        }
    }
}
