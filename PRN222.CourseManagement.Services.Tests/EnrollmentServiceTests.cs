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
    /// Tests business rules: BR16, BR17, BR18, BR19, BR20, BR21, BR22, BR23, BR24
    /// </summary>
    [TestFixture]
    public class EnrollmentServiceTests
    {
        /// <summary>
        /// TC16: Test BR16 - Cannot enroll in the same course twice
        /// Given: Student is already enrolled in course CS101
        /// When: Attempting to enroll the same student in CS101 again
        /// Then: Operation should fail with duplicate enrollment error
        /// </summary>
        [Test]
        public void TC16_EnrollStudent_DuplicateEnrollment_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC16_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Student already enrolled in course
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                EnrollDate = DateTime.Now
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            // When: Attempting to enroll again
            var result = service.EnrollStudent(1, 1, DateTime.Now);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already enrolled"));

            context.Dispose();
        }

        /// <summary>
        /// TC17: Test BR17 - Maximum 10 courses per student (Updated)
        /// Given: Student is enrolled in 10 courses
        /// When: Attempting to enroll in an 11th course
        /// Then: Operation should fail with maximum courses error
        /// </summary>
        [Test]
        public void TC17_EnrollStudent_MaximumCoursesExceeded_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC17_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Student enrolled in 10 courses
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };

            context.Departments.Add(dept);
            context.Students.Add(student);

            // Add 10 courses and enrollments
            for (int i = 1; i <= 10; i++)
            {
                var course = new Course
                {
                    CourseId = i,
                    CourseCode = $"CS10{i}",
                    Title = $"Course {i}",
                    Credits = 3,
                    DepartmentId = 1
                };
                context.Courses.Add(course);
                context.Enrollments.Add(new Enrollment
                {
                    StudentId = 1,
                    CourseId = i,
                    EnrollDate = DateTime.Now
                });
            }

            // Add 11th course
            var eleventhCourse = new Course
            {
                CourseId = 11,
                CourseCode = "CS111",
                Title = "Course 11",
                Credits = 3,
                DepartmentId = 1
            };
            context.Courses.Add(eleventhCourse);
            context.SaveChanges();

            // When: Attempting to enroll in 11th course
            var result = service.EnrollStudent(1, 11, DateTime.Now);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("more than 10 courses"));

            context.Dispose();
        }

        /// <summary>
        /// TC18: Test BR18 - Enroll date cannot be in the past
        /// Given: Attempting to enroll with a past date
        /// When: Enrolling student
        /// Then: Operation should fail with date validation error
        /// </summary>
        [Test]
        public void TC18_EnrollStudent_PastDate_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC18_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Setup
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            // Given: Past enrollment date
            var pastDate = DateTime.Now.AddDays(-1);

            // When: Attempting to enroll with past date
            var result = service.EnrollStudent(1, 1, pastDate);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("cannot be in the past"));

            context.Dispose();
        }

        /// <summary>
        /// TC19: Test BR19 - Cross-departmental enrollment (Updated)
        /// Given: Student in Computer Science department
        /// When: Attempting to enroll in a Mathematics course
        /// Then: Operation should succeed (Rule relaxed)
        /// </summary>
        [Test]
        public void TC19_EnrollStudent_DifferentDepartment_ShouldSucceed()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC19_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Student in CS dept, course in Math dept
            var csDept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var mathDept = new Department { DepartmentId = 2, Name = "Mathematics" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1, // CS Dept
                DateOfBirth = DateTime.Now.AddYears(-20),
                IsActive = true
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "MATH101",
                Title = "Calculus",
                Credits = 3,
                DepartmentId = 2, // Math Dept
                Status = CourseStatus.Active
            };

            context.Departments.AddRange(csDept, mathDept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            // When: Attempting to enroll
            var result = service.EnrollStudent(1, 1, DateTime.Now);

            // Then: Should succeed because BR19 is relaxed
            Assert.That(result.IsSuccess, Is.True);

            context.Dispose();
        }

        /// <summary>
        /// TC20: Test BR20 - Student and course must exist
        /// Given: Attempting to enroll with non-existent student ID
        /// When: Enrolling
        /// Then: Operation should fail with student not found error
        /// </summary>
        [Test]
        public void TC20_EnrollStudent_NonExistentStudent_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC20_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Only course exists, student doesn't
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };

            context.Departments.Add(dept);
            context.Courses.Add(course);
            context.SaveChanges();

            // When: Attempting to enroll non-existent student
            var result = service.EnrollStudent(999, 1, DateTime.Now);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));

            context.Dispose();
        }

        /// <summary>
        /// TC21: Test BR21 - Can only assign grade after enrollment exists
        /// Given: No enrollment exists for student and course
        /// When: Attempting to assign a grade
        /// Then: Operation should fail with enrollment not found error
        /// </summary>
        [Test]
        public void TC21_AssignGrade_NoEnrollment_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC21_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: No enrollment exists
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            // When: Attempting to assign grade without enrollment
            var result = service.AssignGrade(1, 1, 8.5m);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));

            context.Dispose();
        }

        /// <summary>
        /// TC22: Test BR22 - Grade must be in range 0..10
        /// Given: An existing enrollment
        /// When: Attempting to assign a grade of 11
        /// Then: Operation should fail with grade validation error
        /// </summary>
        [Test]
        public void TC22_AssignGrade_InvalidRange_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC22_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Enrollment exists
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                EnrollDate = DateTime.Now
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            // When: Attempting to assign invalid grade
            var result = service.AssignGrade(1, 1, 11m); // Invalid

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("between 0 and 10"));

            context.Dispose();
        }

        /// <summary>
        /// TC23: Test BR23 - Grade cannot be updated once finalized
        /// Given: An enrollment with a finalized grade
        /// When: Attempting to update the grade
        /// Then: Operation should fail with finalized error
        /// </summary>
        [Test]
        public void TC23_AssignGrade_GradeFinalized_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC23_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Enrollment with finalized grade
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };
            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };
            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                EnrollDate = DateTime.Now,
                Grade = 8.5m,
                IsGradeFinalized = true // Finalized
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            // When: Attempting to update finalized grade
            var result = service.AssignGrade(1, 1, 9.0m);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("finalized"));

            context.Dispose();
        }

        /// <summary>
        /// TC24: Test BR24 - Enrollment operations must be transactional (rollback test)
        /// Given: Attempting to enroll with validation error
        /// When: Transaction fails
        /// Then: Database should rollback and remain unchanged
        /// NOTE: Using SQLite in-memory to test real transactions (EF InMemory doesn't support transactions)
        /// </summary>
        [Test]
        public void TC24_EnrollStudent_TransactionRollback_ShouldRollback()
        {
            // Arrange - Using SQLite for real transaction support
            var (context, connection) = TestHelper.CreateSqliteInMemoryContext();
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            try
            {
                // Given: Student under 16 - will cause failure
                var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
                var student = new Student
                {
                    StudentId = 1,
                    StudentCode = "STU001",
                    FullName = "Young Kid",
                    Email = "kid@test.com",
                    DepartmentId = 1,
                    DateOfBirth = DateTime.Now.AddYears(-10) // 10 tuổi < 16
                };
                var course = new Course
                {
                    CourseId = 1,
                    CourseCode = "CS101",
                    Title = "Programming",
                    Credits = 3,
                    DepartmentId = 1,
                    Status = CourseStatus.Active
                };

                context.Departments.Add(dept);
                context.Students.Add(student);
                context.Courses.Add(course);
                context.SaveChanges();

                // Record initial enrollment count
                var initialCount = context.Enrollments.Count();

                // When: Attempting enrollment that should fail due to age and rollback
                var result = service.EnrollStudent(1, 1, DateTime.Now);

                // Then: Should fail
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Does.Contain("16"));

                // Verify rollback - enrollment count should be unchanged
                var finalCount = context.Enrollments.Count();
                Assert.That(finalCount, Is.EqualTo(initialCount), "Transaction should have rolled back");
            }
            finally
            {
                context.Dispose();
                connection.Dispose();
            }
        }

        /// <summary>
        /// TC25: Test BR25 - Services must return failure result instead of throwing exceptions
        /// Given: Invalid operation (e.g., null student ID)
        /// When: Service method is called
        /// Then: Should return ServiceResult with IsSuccess=false, not throw exception
        /// </summary>
        [Test]
        public void TC25_ServiceMethods_ShouldReturnResultNotThrow()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC25_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            // Given: Empty database

            // When: Calling service method with invalid data
            // Then: Should NOT throw, should return failure result
            Assert.DoesNotThrow(() =>
            {
                var result = service.EnrollStudent(999, 999, DateTime.Now);
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message, Is.Not.Empty);
            });

            context.Dispose();
        }

        [Test]
        public void TC26_EnrollStudent_StudentUnder16_ShouldFail()
        {
            var context = TestHelper.CreateInMemoryContext("TC26_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "Young Student",
                Email = "young@test.com",
                DepartmentId = 1,
                DateOfBirth = DateTime.Now.AddYears(-15) // < 16 tuổi
            };

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1,
                Status = CourseStatus.Active
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            // When
            var result = service.EnrollStudent(1, 1, DateTime.Now);

            // Then
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("16"));

            context.Dispose();
        }
        [Test]
        public void TC27_EnrollStudent_CourseWithZeroCredits_ShouldFail()
        {
            var context = TestHelper.CreateInMemoryContext("TC27_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS000",
                Title = "Invalid Course",
                Credits = 0,
                DepartmentId = 1
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            var result = service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("credit"));

            context.Dispose();
        }
        [Test]
        public void TC28_EnrollStudent_InactiveCourse_ShouldFail()
        {
            var context = TestHelper.CreateInMemoryContext("TC28_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1,
                Status = CourseStatus.Inactive
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            var result = service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("inactive"));

            context.Dispose();
        }

        [Test]
        public void TC29_EnrollStudent_InactiveStudent_ShouldFail()
        {
            var context = TestHelper.CreateInMemoryContext("TC29_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1,
                IsActive = false
            };

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            var result = service.EnrollStudent(1, 1, DateTime.Now);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("inactive"));

            context.Dispose();
        }
        [Test]
        public void TC30_AssignGrade_OutsideGradingPeriod_ShouldFail()
        {
            var context = TestHelper.CreateInMemoryContext("TC30_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new EnrollmentService(unitOfWork);

            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@test.com",
                DepartmentId = 1
            };

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Programming",
                Credits = 3,
                DepartmentId = 1
            };

            var enrollment = new Enrollment
            {
                StudentId = 1,
                CourseId = 1,
                EnrollDate = DateTime.Now.AddDays(-600) // ngoài 500 ngày
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            var result = service.AssignGrade(1, 1, 8.5m);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("grading period"));

            context.Dispose();
        }

    }


}
