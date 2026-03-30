using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Services.Implementations;
using NUnit.Framework;
using System.Linq;

namespace PRN222.CourseManagement.Services.Tests
{
    /// <summary>
    /// Student Service Tests
    /// Tests business rules: BR05, BR06, BR07, BR08, BR09, BR10
    /// </summary>
    [TestFixture]
    public class StudentServiceTests
    {
        private CourseManagementContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private StudentService _service = null!;
        private bool _disposed = false;

        [SetUp]
        public void SetUp()
        {
            _context = TestHelper.CreateInMemoryContext("StudentTests_" + System.Guid.NewGuid());
            _unitOfWork = new UnitOfWork(_context);
            _service = new StudentService(_unitOfWork);

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
        // GET ALL / GET BY ID / GET BY CODE
        // =========================================================

        [Test]
        public void GetAllStudents_EmptyDatabase_ReturnsEmptyList()
        {
            var result = _service.GetAllStudents();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Any(), Is.False);
        }

        [Test]
        public void GetAllStudents_WithStudents_ReturnsAll()
        {
            _context.Students.Add(new Student { StudentCode = "S001", FullName = "Alice", Email = "a@test.com", DepartmentId = 1 });
            _context.Students.Add(new Student { StudentCode = "S002", FullName = "Bob", Email = "b@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.GetAllStudents();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetStudentById_ExistingId_ReturnsStudent()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "S001", FullName = "Alice", Email = "a@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.GetStudentById(1);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.StudentCode, Is.EqualTo("S001"));
        }

        [Test]
        public void GetStudentById_NonExistingId_ReturnsFailure()
        {
            var result = _service.GetStudentById(999);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public void GetStudentByCode_ExistingCode_ReturnsStudent()
        {
            _context.Students.Add(new Student { StudentCode = "S001", FullName = "Alice", Email = "a@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.GetStudentByCode("S001");
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void GetStudentByCode_NonExistingCode_ReturnsFailure()
        {
            var result = _service.GetStudentByCode("INVALID");
            Assert.That(result.IsSuccess, Is.False);
        }

        // =========================================================
        // ADD (BR05, BR06, BR07, BR08, BR09)
        // =========================================================

        /// <summary>TC05: BR05 - StudentCode must be unique</summary>
        [Test]
        public void TC05_AddStudent_DuplicateCode_ShouldFail()
        {
            _context.Students.Add(new Student { StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.AddStudent(new Student { StudentCode = "STU001", FullName = "Jane Smith", Email = "jane@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));
        }

        /// <summary>TC06: BR06 - Student must belong to existing department</summary>
        [Test]
        public void TC06_AddStudent_NonExistentDepartment_ShouldFail()
        {
            var result = _service.AddStudent(new Student { StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 999 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        /// <summary>TC07: BR07 - Full name not null/empty</summary>
        [Test]
        public void TC07_AddStudent_EmptyName_ShouldFail()
        {
            var result = _service.AddStudent(new Student { StudentCode = "STU001", FullName = "", Email = "john@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("cannot be empty"));
        }

        [Test]
        public void AddStudent_WhitespaceName_ShouldFail()
        {
            var result = _service.AddStudent(new Student { StudentCode = "STU001", FullName = "   ", Email = "x@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        /// <summary>TC08: BR08 - Full name length >= 3</summary>
        [Test]
        public void TC08_AddStudent_ShortName_ShouldFail()
        {
            var result = _service.AddStudent(new Student { StudentCode = "STU001", FullName = "Jo", Email = "john@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("at least 3 characters"));
        }

        /// <summary>TC09: BR09 - Email if provided must be unique</summary>
        [Test]
        public void TC09_AddStudent_DuplicateEmail_ShouldFail()
        {
            _context.Students.Add(new Student { StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.AddStudent(new Student { StudentCode = "STU002", FullName = "Jane Smith", Email = "john@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already in use"));
        }

        [Test]
        public void AddStudent_ValidData_ShouldSucceed()
        {
            var result = _service.AddStudent(new Student { StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Does.Contain("added successfully"));
        }

        // =========================================================
        // UPDATE (BR05, BR06, BR07, BR08, BR09)
        // =========================================================

        [Test]
        public void UpdateStudent_ValidData_ShouldSucceed()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.UpdateStudent(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Updated", Email = "john@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void UpdateStudent_NonExistentStudent_ShouldFail()
        {
            var result = _service.UpdateStudent(new Student { StudentId = 999, StudentCode = "X", FullName = "Test Person", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public void UpdateStudent_EmptyName_ShouldFail()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.UpdateStudent(new Student { StudentId = 1, StudentCode = "STU001", FullName = "", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateStudent_ShortName_ShouldFail()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.UpdateStudent(new Student { StudentId = 1, StudentCode = "STU001", FullName = "Jo", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateStudent_DuplicateCode_ShouldFail()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "j@test.com", DepartmentId = 1 });
            _context.Students.Add(new Student { StudentId = 2, StudentCode = "STU002", FullName = "Jane Smith", Email = "ja@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.UpdateStudent(new Student { StudentId = 2, StudentCode = "STU001", FullName = "Jane Updated", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateStudent_DuplicateEmail_ShouldFail()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.Students.Add(new Student { StudentId = 2, StudentCode = "STU002", FullName = "Jane Smith", Email = "jane@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.UpdateStudent(new Student { StudentId = 2, StudentCode = "STU002", FullName = "Jane Updated", Email = "john@test.com", DepartmentId = 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void UpdateStudent_NonExistentDepartment_ShouldFail()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "j@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.UpdateStudent(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", DepartmentId = 999 });

            Assert.That(result.IsSuccess, Is.False);
        }

        // =========================================================
        // DELETE (BR10)
        // =========================================================

        /// <summary>TC10: BR10 - Cannot delete student if has enrollments</summary>
        [Test]
        public void TC10_DeleteStudent_HasEnrollments_ShouldFail()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1 });
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = System.DateTime.Now });
            _context.SaveChanges();

            var result = _service.DeleteStudent(1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has active enrollments"));
        }

        [Test]
        public void DeleteStudent_NoEnrollments_ShouldSucceed()
        {
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "john@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.DeleteStudent(1);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void DeleteStudent_NonExistentStudent_ShouldFail()
        {
            var result = _service.DeleteStudent(999);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("not found"));
        }
    }
}
