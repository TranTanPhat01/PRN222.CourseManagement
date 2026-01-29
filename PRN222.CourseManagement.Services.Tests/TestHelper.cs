using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System;

namespace PRN222.CourseManagement.Services.Tests
{
    /// <summary>
    /// Test helper for creating in-memory databases and test data
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Creates a unique in-memory database context for testing
        /// </summary>
        public static CourseManagementContext CreateInMemoryContext(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new CourseManagementContext(options);
        }

        /// <summary>
        /// Creates a SQLite in-memory database context for transaction testing
        /// SQLite supports real transactions unlike EF InMemory provider
        /// </summary>
        public static (CourseManagementContext context, SqliteConnection connection) CreateSqliteInMemoryContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseSqlite(connection)
                .Options;

            var context = new CourseManagementContext(options);
            context.Database.EnsureCreated();

            return (context, connection);
        }

        /// <summary>
        /// Creates a UnitOfWork with a fresh in-memory database
        /// </summary>
        public static IUnitOfWork CreateUnitOfWork(string dbName = null)
        {
            var context = CreateInMemoryContext(dbName);
            return new UnitOfWork(context);
        }

        /// <summary>
        /// Seeds test data for common scenarios
        /// </summary>
        public static void SeedTestData(CourseManagementContext context)
        {
            var department = new Department
            {
                DepartmentId = 1,
                Name = "Computer Science",
                Description = "CS Department"
            };

            var department2 = new Department
            {
                DepartmentId = 2,
                Name = "Mathematics",
                Description = "Math Department"
            };

            var student = new Student
            {
                StudentId = 1,
                StudentCode = "STU001",
                FullName = "John Doe",
                Email = "john@example.com",
                DepartmentId = 1
            };

            var course = new Course
            {
                CourseId = 1,
                CourseCode = "CS101",
                Title = "Introduction to Programming",
                Credits = 3,
                DepartmentId = 1,
                Status = CourseStatus.Active
            };

            context.Departments.AddRange(department, department2);
            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();
        }
    }
}
