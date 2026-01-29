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
        /// <summary>
        /// TC05: Test BR05 - StudentCode must be unique
        /// Given: A student with code "STU001" exists
        /// When: Attempting to add another student with the same code
        /// Then: Operation should fail with appropriate error message
        /// </summary>
        [Test]
        public void TC05_AddStudent_DuplicateCode_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC05_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new StudentService(unitOfWork);

            // Setup department
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            context.Departments.Add(dept);
            context.SaveChanges();

            // Given: Student with code "STU001" exists
            var existing = new Student 
            { 
                StudentCode = "STU001", 
                FullName = "John Doe", 
                Email = "john@test.com",
                DepartmentId = 1 
            };
            context.Students.Add(existing);
            context.SaveChanges();

            // When: Attempting to add duplicate student code
            var newStudent = new Student 
            { 
                StudentCode = "STU001", 
                FullName = "Jane Smith", 
                Email = "jane@test.com",
                DepartmentId = 1 
            };
            var result = service.AddStudent(newStudent);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));

            context.Dispose();
        }

        /// <summary>
        /// TC06: Test BR06 - Student must belong to exactly one department (department must exist)
        /// Given: Attempting to add a student to a non-existent department
        /// When: Adding the student
        /// Then: Operation should fail indicating department doesn't exist
        /// </summary>
        [Test]
        public void TC06_AddStudent_NonExistentDepartment_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC06_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new StudentService(unitOfWork);

            // Given: Student with non-existent department
            var student = new Student 
            { 
                StudentCode = "STU001", 
                FullName = "John Doe", 
                Email = "john@test.com",
                DepartmentId = 999 // Non-existent
            };

            // When: Attempting to add student
            var result = service.AddStudent(student);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));

            context.Dispose();
        }

        /// <summary>
        /// TC07: Test BR07 - Full name not null/empty
        /// Given: A student with empty full name
        /// When: Attempting to add the student
        /// Then: Operation should fail with validation error
        /// </summary>
        [Test]
        public void TC07_AddStudent_EmptyName_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC07_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new StudentService(unitOfWork);

            // Setup department
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            context.Departments.Add(dept);
            context.SaveChanges();

            // Given: Student with empty name
            var student = new Student 
            { 
                StudentCode = "STU001", 
                FullName = "", 
                Email = "john@test.com",
                DepartmentId = 1 
            };

            // When: Attempting to add
            var result = service.AddStudent(student);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("cannot be empty"));

            context.Dispose();
        }

        /// <summary>
        /// TC08: Test BR08 - Full name length >= 3
        /// Given: A student with name less than 3 characters
        /// When: Attempting to add the student
        /// Then: Operation should fail with validation error
        /// </summary>
        [Test]
        public void TC08_AddStudent_ShortName_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC08_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new StudentService(unitOfWork);

            // Setup department
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            context.Departments.Add(dept);
            context.SaveChanges();

            // Given: Student with short name
            var student = new Student 
            { 
                StudentCode = "STU001", 
                FullName = "Jo", 
                Email = "john@test.com",
                DepartmentId = 1 
            };

            // When: Attempting to add
            var result = service.AddStudent(student);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("at least 3 characters"));

            context.Dispose();
        }

        /// <summary>
        /// TC09: Test BR09 - Email if provided must be unique
        /// Given: A student with email "john@test.com" exists
        /// When: Attempting to add another student with same email
        /// Then: Operation should fail with duplicate email error
        /// </summary>
        [Test]
        public void TC09_AddStudent_DuplicateEmail_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC09_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new StudentService(unitOfWork);

            // Setup department
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            context.Departments.Add(dept);
            context.SaveChanges();

            // Given: Student with email exists
            var existing = new Student 
            { 
                StudentCode = "STU001", 
                FullName = "John Doe", 
                Email = "john@test.com",
                DepartmentId = 1 
            };
            context.Students.Add(existing);
            context.SaveChanges();

            // When: Attempting to add student with duplicate email
            var newStudent = new Student 
            { 
                StudentCode = "STU002", 
                FullName = "Jane Smith", 
                Email = "john@test.com",
                DepartmentId = 1 
            };
            var result = service.AddStudent(newStudent);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already in use"));

            context.Dispose();
        }

        /// <summary>
        /// TC10: Test BR10 - Cannot delete student if has enrollments
        /// Given: A student with active enrollments
        /// When: Attempting to delete the student
        /// Then: Operation should fail indicating enrollments exist
        /// </summary>
        [Test]
        public void TC10_DeleteStudent_HasEnrollments_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC10_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new StudentService(unitOfWork);

            // Given: Student with enrollment
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
                EnrollDate = System.DateTime.Now 
            };

            context.Departments.Add(dept);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            // When: Attempting to delete student
            var result = service.DeleteStudent(1);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has active enrollments"));

            context.Dispose();
        }
    }
}
