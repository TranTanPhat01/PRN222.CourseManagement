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
        /// <summary>
        /// TC11: Test BR11 - CourseCode must be unique
        /// Given: A course with code "CS101" exists
        /// When: Attempting to add another course with the same code
        /// Then: Operation should fail with appropriate error message
        /// </summary>
        [Test]
        public void TC11_AddCourse_DuplicateCode_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC11_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new CourseService(unitOfWork);

            // Setup department
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            context.Departments.Add(dept);
            context.SaveChanges();

            // Given: Course with code "CS101" exists
            var existing = new Course 
            { 
                CourseCode = "CS101", 
                Title = "Programming", 
                Credits = 3,
                DepartmentId = 1 
            };
            context.Courses.Add(existing);
            context.SaveChanges();

            // When: Attempting to add duplicate course code
            var newCourse = new Course 
            { 
                CourseCode = "CS101", 
                Title = "Advanced Programming", 
                Credits = 3,
                DepartmentId = 1 
            };
            var result = service.AddCourse(newCourse);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));

            context.Dispose();
        }

        /// <summary>
        /// TC12: Test BR12 - Course must belong to exactly one department
        /// Given: Attempting to add a course to a non-existent department
        /// When: Adding the course
        /// Then: Operation should fail indicating department doesn't exist
        /// </summary>
        [Test]
        public void TC12_AddCourse_NonExistentDepartment_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC12_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new CourseService(unitOfWork);

            // Given: Course with non-existent department
            var course = new Course 
            { 
                CourseCode = "CS101", 
                Title = "Programming", 
                Credits = 3,
                DepartmentId = 999 // Non-existent
            };

            // When: Attempting to add course
            var result = service.AddCourse(course);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("does not exist"));

            context.Dispose();
        }

        /// <summary>
        /// TC13: Test BR13 - Credits must be between 1 and 6
        /// Given: A course with 0 credits
        /// When: Attempting to add the course
        /// Then: Operation should fail with validation error
        /// </summary>
        [Test]
        public void TC13_AddCourse_InvalidCredits_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC13_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new CourseService(unitOfWork);

            // Setup department
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            context.Departments.Add(dept);
            context.SaveChanges();

            // Given: Course with invalid credits
            var course = new Course 
            { 
                CourseCode = "CS101", 
                Title = "Programming", 
                Credits = 0, // Invalid
                DepartmentId = 1 
            };

            // When: Attempting to add
            var result = service.AddCourse(course);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("between 1 and 6"));

            context.Dispose();
        }

        /// <summary>
        /// TC14: Test BR14 - Cannot delete course if has enrollments
        /// Given: A course with active enrollments
        /// When: Attempting to delete the course
        /// Then: Operation should fail indicating enrollments exist
        /// </summary>
        [Test]
        public void TC14_DeleteCourse_HasEnrollments_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC14_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new CourseService(unitOfWork);

            // Given: Course with enrollment
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

            // When: Attempting to delete course
            var result = service.DeleteCourse(1);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has active enrollments"));

            context.Dispose();
        }

        /// <summary>
        /// TC15: Test BR15 - Cannot update course if inactive or archived
        /// Given: A course with Inactive status
        /// When: Attempting to update the course
        /// Then: Operation should fail indicating course is inactive
        /// </summary>
        [Test]
        public void TC15_UpdateCourse_InactiveStatus_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC15_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new CourseService(unitOfWork);

            // Given: Inactive course
            var dept = new Department { DepartmentId = 1, Name = "Computer Science" };
            var course = new Course 
            { 
                CourseId = 1,
                CourseCode = "CS101", 
                Title = "Programming", 
                Credits = 3,
                DepartmentId = 1,
                Status = CourseStatus.Inactive // Inactive
            };

            context.Departments.Add(dept);
            context.Courses.Add(course);
            context.SaveChanges();

            // When: Attempting to update
            course.Title = "Updated Programming";
            var result = service.UpdateCourse(course);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("Inactive"));

            context.Dispose();
        }
    }
}
