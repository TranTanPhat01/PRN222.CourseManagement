using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Services.Implementations;
using NUnit.Framework;
using System;
using System.Linq;

namespace PRN222.CourseManagement.Services.Tests
{
    /// <summary>
    /// Enrollment Service Tests
    /// Tests business rules: BR16-BR24
    /// </summary>
    [TestFixture]
    public class EnrollmentServiceTests
    {
        private CourseManagementContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private EnrollmentService _service = null!;
        private bool _disposed = false;

        [SetUp]
        public void SetUp()
        {
            _context = TestHelper.CreateInMemoryContext("EnrollTests_" + Guid.NewGuid());
            _unitOfWork = new UnitOfWork(_context);
            _service = new EnrollmentService(_unitOfWork);
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

        private void AddBaseData(bool studentActive = true, CourseStatus courseStatus = CourseStatus.Active, int studentAge = 20, int courseCredits = 3)
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.Students.Add(new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1,
                IsActive = studentActive,
                DateOfBirth = DateTime.Now.AddYears(-studentAge)
            });
            _context.Courses.Add(new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = courseCredits,
                DepartmentId = 1,
                Status = courseStatus
            });
            _context.SaveChanges();
        }

        // =========================================================
        // GET ALL / GET BY STUDENT / GET BY COURSE
        // =========================================================

        [Test]
        public void GetAllEnrollments_EmptyDatabase_ReturnsEmpty()
        {
            var result = _service.GetAllEnrollments();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Any(), Is.False);
        }

        [Test]
        public void GetAllEnrollments_WithData_ReturnsAll()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.GetAllEnrollments();
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetEnrollment_ExistingEnrollment_ReturnsEnrollment()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.GetEnrollment(1, 1);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void GetEnrollment_NonExistingEnrollment_ReturnsFailure()
        {
            var result = _service.GetEnrollment(999, 999);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void GetEnrollmentsByStudent_ReturnsCorrectData()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.GetEnrollmentsByStudent(1);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetEnrollmentsByCourse_ReturnsCorrectData()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.GetEnrollmentsByCourse(1);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data!.Count(), Is.EqualTo(1));
        }

        // =========================================================
        // ENROLL (BR16-BR20, BR26-BR29)
        // =========================================================

        [Test]
        public void EnrollStudent_ValidData_ShouldSucceed()
        {
            AddBaseData(studentActive: true, courseStatus: CourseStatus.Active, studentAge: 20, courseCredits: 3);

            var result = _service.EnrollStudent(1, 1, DateTime.Now);
            Assert.That(result.IsSuccess, Is.True);
        }

        /// <summary>TC16: BR16 - Cannot enroll in same course twice</summary>
        [Test]
        public void TC16_EnrollStudent_DuplicateEnrollment_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already enrolled"));
        }

        /// <summary>TC17: BR17 - Maximum 10 courses per student</summary>
        [Test]
        public void TC17_EnrollStudent_MaximumCoursesExceeded_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "j@test.com", DepartmentId = 1 });

            for (int i = 1; i <= 10; i++)
            {
                _context.Courses.Add(new Course { CourseId = i, CourseCode = $"CS{i:D3}", Title = $"Course {i}", Credits = 3, DepartmentId = 1 });
                _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = i, EnrollDate = DateTime.Now });
            }

            _context.Courses.Add(new Course { CourseId = 11, CourseCode = "CS111", Title = "Course 11", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.EnrollStudent(1, 11, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("more than 10 courses"));
        }

        /// <summary>TC18: BR18 - Enroll date cannot be in the past</summary>
        [Test]
        public void TC18_EnrollStudent_PastDate_ShouldFail()
        {
            AddBaseData();

            var result = _service.EnrollStudent(1, 1, DateTime.Now.AddDays(-1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("cannot be in the past"));
        }

        /// <summary>TC19: BR19 - Cross-departmental enrollment should succeed (rule relaxed)</summary>
        [Test]
        public void TC19_EnrollStudent_DifferentDepartment_ShouldSucceed()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "Computer Science" });
            _context.Departments.Add(new Department { DepartmentId = 2, Name = "Mathematics" });
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "j@test.com", DepartmentId = 1, DateOfBirth = DateTime.Now.AddYears(-20), IsActive = true });
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "MATH101", Title = "Calculus", Credits = 3, DepartmentId = 2, Status = CourseStatus.Active });
            _context.SaveChanges();

            var result = _service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.True);
        }

        /// <summary>TC20: BR20 - Student and course must exist</summary>
        [Test]
        public void TC20_EnrollStudent_NonExistentStudent_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "CS" });
            _context.Courses.Add(new Course { CourseId = 1, CourseCode = "CS101", Title = "Prog", Credits = 3, DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.EnrollStudent(999, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        [Test]
        public void EnrollStudent_NonExistentCourse_ShouldFail()
        {
            _context.Departments.Add(new Department { DepartmentId = 1, Name = "CS" });
            _context.Students.Add(new Student { StudentId = 1, StudentCode = "STU001", FullName = "John Doe", Email = "j@test.com", DepartmentId = 1 });
            _context.SaveChanges();

            var result = _service.EnrollStudent(1, 999, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        [Test]
        public void TC26_EnrollStudent_StudentUnder16_ShouldFail()
        {
            AddBaseData(studentAge: 15, courseStatus: CourseStatus.Active);

            var result = _service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("16"));
        }

        [Test]
        public void TC27_EnrollStudent_CourseWithZeroCredits_ShouldFail()
        {
            AddBaseData(courseCredits: 0, studentAge: 20);

            var result = _service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("credit"));
        }

        [Test]
        public void TC28_EnrollStudent_InactiveCourse_ShouldFail()
        {
            AddBaseData(courseStatus: CourseStatus.Inactive);

            var result = _service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("inactive"));
        }

        [Test]
        public void TC29_EnrollStudent_InactiveStudent_ShouldFail()
        {
            AddBaseData(studentActive: false);

            var result = _service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("inactive"));
        }

        // =========================================================
        // ASSIGN GRADE (BR21-BR23, BR30)
        // =========================================================

        /// <summary>TC21: BR21 - Can only assign grade after enrollment exists</summary>
        [Test]
        public void TC21_AssignGrade_NoEnrollment_ShouldFail()
        {
            AddBaseData();

            var result = _service.AssignGrade(1, 1, 8.5m);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        /// <summary>TC22: BR22 - Grade must be in range 0..10</summary>
        [Test]
        public void TC22_AssignGrade_InvalidRange_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.AssignGrade(1, 1, 11m);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("between 0 and 10"));
        }

        [Test]
        public void AssignGrade_NegativeGrade_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.AssignGrade(1, 1, -1m);

            Assert.That(result.IsSuccess, Is.False);
        }

        /// <summary>TC23: BR23 - Grade cannot be updated once finalized</summary>
        [Test]
        public void TC23_AssignGrade_GradeFinalized_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now, Grade = 8.5m, IsGradeFinalized = true });
            _context.SaveChanges();

            var result = _service.AssignGrade(1, 1, 9.0m);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("finalized"));
        }

        [Test]
        public void AssignGrade_ValidGrade_ShouldSucceed()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.AssignGrade(1, 1, 8.5m);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void TC30_AssignGrade_OutsideGradingPeriod_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now.AddDays(-600) });
            _context.SaveChanges();

            var result = _service.AssignGrade(1, 1, 8.5m);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("grading period"));
        }

        // =========================================================
        // FINALIZE GRADE
        // =========================================================

        [Test]
        public void FinalizeGrade_ValidEnrollmentWithGrade_ShouldSucceed()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now, Grade = 9.0m });
            _context.SaveChanges();

            var result = _service.FinalizeGrade(1, 1);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void FinalizeGrade_NoEnrollment_ShouldFail()
        {
            var result = _service.FinalizeGrade(1, 1);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void FinalizeGrade_NoGradeAssigned_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now, Grade = null });
            _context.SaveChanges();

            var result = _service.FinalizeGrade(1, 1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("no grade"));
        }

        [Test]
        public void FinalizeGrade_AlreadyFinalized_ShouldFail()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now, Grade = 9.0m, IsGradeFinalized = true });
            _context.SaveChanges();

            var result = _service.FinalizeGrade(1, 1);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already finalized"));
        }

        // =========================================================
        // UNENROLL (BR24)
        // =========================================================

        [Test]
        public void UnenrollStudent_ExistingEnrollment_ShouldSucceed()
        {
            AddBaseData();
            _context.Enrollments.Add(new Enrollment { StudentId = 1, CourseId = 1, EnrollDate = DateTime.Now });
            _context.SaveChanges();

            var result = _service.UnenrollStudent(1, 1);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void UnenrollStudent_NonExistentEnrollment_ShouldFail()
        {
            var result = _service.UnenrollStudent(1, 1);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        /// <summary>TC24: BR24 - Enrollment operations must be transactional (rollback test)</summary>
        [Test]
        public void TC24_EnrollStudent_TransactionRollback_ShouldRollback()
        {
            var (context, connection) = TestHelper.CreateSqliteInMemoryContext();
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            try
            {
                var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
                var student = new Student { StudentId = 1, StudentCode = "STU001", FullName = "Young Kid", Email = "kid@test.com", DepartmentId = 1, DateOfBirth = DateTime.Now.AddYears(-10) };
                var course = new Course { CourseId = 1, CourseCode = "CS101", Title = "Programming", Credits = 3, DepartmentId = 1, Status = CourseStatus.Active };

                context.Departments.Add(dept);
                context.Students.Add(student);
                context.Courses.Add(course);
                context.SaveChanges();

                var initialCount = context.Enrollments.Count();
                var result = service.EnrollStudent(1, 1, DateTime.Now);

                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Does.Contain("16"));

                var finalCount = context.Enrollments.Count();
                Assert.That(finalCount, Is.EqualTo(initialCount));
            }
            finally
            {
                context.Dispose();
                connection.Dispose();
            }
        }

        /// <summary>TC25: BR25 - Services must return result not throw exceptions</summary>
        [Test]
        public void TC25_ServiceMethods_ShouldReturnResultNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = _service.EnrollStudent(999, 999, DateTime.Now);
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Is.Not.Empty);
            });
        }
    }
}
