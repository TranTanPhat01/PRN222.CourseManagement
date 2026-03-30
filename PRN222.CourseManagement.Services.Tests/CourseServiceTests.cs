using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Services.Implementations;
using NUnit.Framework;
using System.Linq;

namespace PRN222.CourseManagement.Services.Tests
{
    /// <summary>
    /// Course Service Tests
    /// Tests business rules: BR11, BR12, BR13, BR14, BR15
    /// </summary>
    [TestFixture]
    public class CourseServiceTests
    {
        private CourseManagementContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private CourseService _service = null!;
        private bool _disposed = false;

        [SetUp]
        public void SetUp()
        {
            _context = TestHelper.CreateInMemoryContext("CourseTests_" + System.Guid.NewGuid());
            _unitOfWork = new UnitOfWork(_context);
            _service = new CourseService(_unitOfWork);

            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            _context.Departments.Add(dept);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            if (!_disposed)
            {
                _unitOfWork.Dispose();
                _context.Dispose();
                _disposed = true;
            }
        }

        // =========================================================
        // GET ALL / GET BY ID
        // =========================================================

        [Test]
        public void GetAllCourses_EmptyDatabase_ReturnsEmptyList()
        {
            var result = _service.GetAllCourses();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Any(), Is.False);
        }

        [Test]
        public void GetAllCourses_WithCourses_ReturnsAll()
        {
            _context.Courses.Add(new Course { CourseCode = "CS101", Title = "Prog", Credits = 3, DepartmentId = 1 });
            _context.Courses.Add(new Course { CourseCode = "CS102", Title = "OOP", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.GetAllCourses();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetCourseById_ExistingId_ReturnsCourse()
        {
            _context.Courses.Add(new Course { CourseId = 10, CourseCode = "CS200", Title = "DSA", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.GetCourseById(10);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.CourseCode, Is.EqualTo("CS200"));
        }

        [Test]
        public void GetCourseById_NonExistingId_ReturnsFailure()
        {
            var result = _service.GetCourseById(999);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public void GetCourseByCode_ExistingCode_ReturnsCourse()
        {
            _context.Courses.Add(new Course { CourseCode = "CS300", Title = "Algo", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.GetCourseByCode("CS300");
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Title, Is.EqualTo("Algo"));
        }

        [Test]
        public void GetCourseByCode_NonExistingCode_ReturnsFailure()
        {
            var result = _service.GetCourseByCode("INVALID");
            Assert.That(result.IsSuccess, Is.False);
        }

        // =========================================================
        // ADD (BR11, BR12, BR13)
        // =========================================================

        /// <summary>TC11: BR11 - CourseCode must be unique</summary>
        [Test]
        public void TC11_AddCourse_DuplicateCode_ShouldFail()
        {
            _context.Courses.Add(new Course { CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.AddCourse(new Course { CourseCode = "CS101", Title = "Advanced Programming", Credits = 3, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));
        }

        /// <summary>TC12: BR12 - Course must belong to existing department</summary>
        [Test]
        public void TC12_AddCourse_NonExistentDepartment_ShouldFail()
        {
            var result = _service.AddCourse(new Course { CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 999 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        /// <summary>TC13: BR13 - Credits must be between 1 and 6</summary>
        [Test]
        public void TC13_AddCourse_ZeroCredits_ShouldFail()
        {
            var result = _service.AddCourse(new Course { CourseCode = "CS101", Title = "Programming", Credits = 0, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("between 1 and 6"));
        }

        [Test]
        public void TC13b_AddCourse_SevenCredits_ShouldFail()
        {
            var result = _service.AddCourse(new Course { CourseCode = "CS101", Title = "Programming", Credits = 7, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("between 1 and 6"));
        }

        [Test]
        public void AddCourse_ValidData_ShouldSucceed()
        {
            var result = _service.AddCourse(new Course { CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Does.Contain("added successfully"));
        }

        // =========================================================
        // UPDATE (BR11, BR12, BR13, BR15)
        // =========================================================

        [Test]
        public void UpdateCourse_ValidData_ShouldSucceed()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active });
            _context.SaveChanges();

            var result = _service.UpdateCourse(new Course { CourseId = 1, CourseCode = "CS101", Title = "Updated Programming", Credits = 4, DepartmentId = 1, Status = CourseStatus.Active });

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void UpdateCourse_NonExistentCourse_ShouldFail()
        {
            var result = _service.UpdateCourse(new Course { CourseId = 999, CourseCode = "CS999", Title = "X", Credits = 3, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        /// <summary>TC15: BR15 - Cannot update inactive course</summary>
        [Test]
        public void TC15_UpdateCourse_InactiveStatus_ShouldFail()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Inactive });
            _context.SaveChanges();

            var result = _service.UpdateCourse(new Course { CourseId = 1, CourseCode = "CS101", Title = "Updated", Credits = 3, DepartmentId = 1, Status = CourseStatus.Inactive });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("Inactive"));
        }

        [Test]
        public void UpdateCourse_ArchivedStatus_ShouldFail()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Archived });
            _context.SaveChanges();

            var result = _service.UpdateCourse(new Course { CourseId = 1, CourseCode = "CS101", Title = "Updated", Credits = 3, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateCourse_DuplicateCodeOnOtherCourse_ShouldFail()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active });
            _context.Courses.Add(new Course { CourseId = 2, CourseCode = "CS102", Title = "OOP", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active });
            _context.SaveChanges();

            var result = _service.UpdateCourse(new Course { CourseId = 2, CourseCode = "CS101", Title = "OOP Updated", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));
        }

        [Test]
        public void UpdateCourse_InvalidCredits_ShouldFail()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active });
            _context.SaveChanges();

            var result = _service.UpdateCourse(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 0, DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateCourse_NonExistentDepartment_ShouldFail()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active });
            _context.SaveChanges();

            var result = _service.UpdateCourse(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 999 });

            Assert.That(result.IsSuccess, Is.False);
        }

        // =========================================================
        // DELETE (BR14)
        // =========================================================

        /// <summary>TC14: BR14 - Cannot delete course with enrollments</summary>
        [Test]
        public void TC14_DeleteCourse_HasEnrollments_ShouldFail()
        {
            var student = new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 };
            _context.Students.Add(student);
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1 });
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = System.DateTime.Now });
            _context.SaveChanges();

            var result = _service.DeleteCourse(1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has active enrollments"));
        }

        [Test]
        public void DeleteCourse_NoEnrollments_ShouldSucceed()
        {
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.DeleteCourse(1);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void DeleteCourse_NonExistentCourse_ShouldFail()
        {
            var result = _service.DeleteCourse(999);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }
    }
}
