using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Services.Implementations;
using NUnit.Framework;
using System.Linq;

namespace PRN222.CourseManagement.Services.Tests
{
    /// <summary>
    /// Department Service Tests
    /// Tests business rules: BR01, BR02, BR03, BR04
    /// </summary>
    [TestFixture]
    public class DepartmentServiceTests
    {
        private CourseManagementContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private DepartmentService _service = null!;
        private bool _disposed = false;

        [SetUp]
        public void SetUp()
        {
            _context = TestHelper.CreateInMemoryContext("DeptTests_" + System.Guid.NewGuid());
            _unitOfWork = new UnitOfWork(_context);
            _service = new DepartmentService(_unitOfWork);
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
        public void GetAllDepartments_EmptyDatabase_ReturnsEmptyList()
        {
            var result = _service.GetAllDepartments();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Any(), Is.False);
        }

        [Test]
        public void GetAllDepartments_WithDepartments_ReturnsAll()
        {
            _context.Departments.Add(new Department { Name = "Computer Science" });
            _context.Departments.Add(new Department { Name = "Mathematics" });
            _context.SaveChanges();

            var result = _service.GetAllDepartments();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetDepartmentById_ExistingId_ReturnsDepartment()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.SaveChanges();

            var result = _service.GetDepartmentById(1);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Name, Is.EqualTo("Computer Science"));
        }

        [Test]
        public void GetDepartmentById_NonExistingId_ReturnsFailure()
        {
            var result = _service.GetDepartmentById(999);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        // =========================================================
        // ADD (BR01, BR02)
        // =========================================================

        [Test]
        public void AddDepartment_ValidData_ShouldSucceed()
        {
            var result = _service.AddDepartment(new Department { Name = "Computer Science", Description = "CS Dept" });
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Does.Contain("added successfully"));
        }

        /// <summary>TC01: BR01 - Department name must be unique</summary>
        [Test]
        public void TC01_AddDepartment_DuplicateName_ShouldFail()
        {
            _context.Departments.Add(new Department { Name = "Computer Science", Description = "CS Dept" });
            _context.SaveChanges();

            var result = _service.AddDepartment(new Department { Name = "Computer Science", Description = "Another CS" });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));
        }

        [Test]
        public void AddDepartment_CaseInsensitiveDuplicate_ShouldFail()
        {
            _context.Departments.Add(new Department { Name = "Computer Science" });
            _context.SaveChanges();

            var result = _service.AddDepartment(new Department { Name = "computer science" });
            Assert.That(result.IsSuccess, Is.False);
        }

        /// <summary>TC02: BR02 - Department name must be >= 3 chars</summary>
        [Test]
        public void TC02_AddDepartment_ShortName_ShouldFail()
        {
            var result = _service.AddDepartment(new Department { Name = "CS", Description = "Too short" });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("at least 3 characters"));
        }

        [Test]
        public void AddDepartment_EmptyName_ShouldFail()
        {
            var result = _service.AddDepartment(new Department { Name = "", Description = "Empty name" });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("cannot be empty"));
        }

        [Test]
        public void AddDepartment_WhitespaceName_ShouldFail()
        {
            var result = _service.AddDepartment(new Department { Name = "   " });
            Assert.That(result.IsSuccess, Is.False);
        }

        // =========================================================
        // UPDATE (BR01, BR02)
        // =========================================================

        [Test]
        public void UpdateDepartment_ValidData_ShouldSucceed()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.SaveChanges();

            var result = _service.UpdateDepartment(new Department { DepartmentId = 1, Name = "Computer Sciences Updated", Description = "Updated desc" });

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void UpdateDepartment_NonExistentDepartment_ShouldFail()
        {
            var result = _service.UpdateDepartment(new Department { DepartmentId = 999, Name = "Math" });
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public void UpdateDepartment_EmptyName_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.SaveChanges();

            var result = _service.UpdateDepartment(new Department { DepartmentId = 1, Name = "" });
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateDepartment_ShortName_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.SaveChanges();

            var result = _service.UpdateDepartment(new Department { DepartmentId = 1, Name = "CS" });
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateDepartment_DuplicateNameOnOther_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.Departments.Add(new Department { DepartmentId = 2, Name = "Mathematics" });
            _context.SaveChanges();

            var result = _service.UpdateDepartment(new Department { DepartmentId = 2, Name = "Computer Science" });
            Assert.That(result.IsSuccess, Is.False);
        }

        // =========================================================
        // DELETE (BR03, BR04)
        // =========================================================

        [Test]
        public void DeleteDepartment_ValidEmpty_ShouldSucceed()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Empty Dept" });
            _context.SaveChanges();

            var result = _service.DeleteDepartment(1);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void DeleteDepartment_NonExistentDepartment_ShouldFail()
        {
            var result = _service.DeleteDepartment(999);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        /// <summary>TC03: BR03 - Cannot delete department with students</summary>
        [Test]
        public void TC03_DeleteDepartment_HasStudents_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.DeleteDepartment(1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has students"));
        }

        /// <summary>TC04: BR04 - Cannot delete department with courses</summary>
        [Test]
        public void TC04_DeleteDepartment_HasCourses_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.DeleteDepartment(1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has courses"));
        }
    }
}
