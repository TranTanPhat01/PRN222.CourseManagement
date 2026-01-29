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
        /// <summary>
        /// TC01: Test BR01 - Department name must be unique
        /// Given: A department with name "Computer Science" exists
        /// When: Attempting to add another department with the same name
        /// Then: Operation should fail with appropriate error message
        /// </summary>
        [Test]
        public void TC01_AddDepartment_DuplicateName_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC01_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new DepartmentService(unitOfWork);

            // Given: Department "Computer Science" exists
            var existingDept = new Department { Name = "Computer Science", Description = "CS Dept" };
            context.Departments.Add(existingDept);
            context.SaveChanges();

            // When: Attempting to add duplicate department
            var newDept = new Department { Name = "Computer Science", Description = "Another CS" };
            var result = service.AddDepartment(newDept);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("already exists"));

            context.Dispose();
        }

        /// <summary>
        /// TC02: Test BR02 - Department name not empty and length >= 3
        /// Given: A new department with name less than 3 characters
        /// When: Attempting to add the department
        /// Then: Operation should fail with validation error
        /// </summary>
        [Test]
        public void TC02_AddDepartment_ShortName_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC02_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new DepartmentService(unitOfWork);

            // Given: Department with short name
            var dept = new Department { Name = "CS", Description = "Too short" };

            // When: Attempting to add
            var result = service.AddDepartment(dept);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("at least 3 characters"));

            context.Dispose();
        }

        /// <summary>
        /// TC03: Test BR03 - Cannot delete department if it has students
        /// Given: A department with enrolled students
        /// When: Attempting to delete the department
        /// Then: Operation should fail indicating students exist
        /// </summary>
        [Test]
        public void TC03_DeleteDepartment_HasStudents_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC03_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new DepartmentService(unitOfWork);

            // Given: Department with students
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
            context.SaveChanges();

            // When: Attempting to delete department
            var result = service.DeleteDepartment(1);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has students"));

            context.Dispose();
        }

        /// <summary>
        /// TC04: Test BR04 - Cannot delete department if it has courses
        /// Given: A department with assigned courses
        /// When: Attempting to delete the department
        /// Then: Operation should fail indicating courses exist
        /// </summary>
        [Test]
        public void TC04_DeleteDepartment_HasCourses_ShouldFail()
        {
            // Arrange
            var context = TestHelper.CreateInMemoryContext("TC04_DB");
            var unitOfWork = new UnitOfWork(context);
            var service = new DepartmentService(unitOfWork);

            // Given: Department with courses
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

            // When: Attempting to delete department
            var result = service.DeleteDepartment(1);

            // Then: Should fail
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("has courses"));

            context.Dispose();
        }
    }
}
