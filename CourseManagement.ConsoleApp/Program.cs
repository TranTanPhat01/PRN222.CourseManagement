using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using CourseManagement.Repositories.Interfaces;
using CourseManagement.Services.Implementations;
using CourseManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

// ========== CONFIG & DI ==========
var services = new ServiceCollection();

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connStr = config.GetConnectionString("CourseManagementDB");
if (string.IsNullOrWhiteSpace(connStr))
    throw new Exception("Missing ConnectionStrings:CourseManagementDB in appsettings.json");

// Register DbContext
services.AddDbContext<CourseManagementContext>(options =>
    options.UseSqlServer(connStr));

// Register Repository Layer
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Service Layer
services.AddScoped<IDepartmentService, DepartmentService>();
services.AddScoped<IStudentService, StudentService>();
services.AddScoped<ICourseService, CourseService>();
services.AddScoped<IEnrollmentService, EnrollmentService>();

var provider = services.BuildServiceProvider();

// ========== MENU LOOP ==========
while (true)
{
    using var scope = provider.CreateScope();
    var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
    var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
    var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
    var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

    Console.Clear();
    Console.WriteLine("====================================");
    Console.WriteLine("   COURSE MANAGEMENT SYSTEM");
    Console.WriteLine("   (Service Layer Implementation)");
    Console.WriteLine("====================================");
    Console.WriteLine("1. Display all departments");
    Console.WriteLine("2. Display all students");
    Console.WriteLine("3. Display all courses");
    Console.WriteLine("4. Add new department");
    Console.WriteLine("5. Add new student");
    Console.WriteLine("6. Add new course");
    Console.WriteLine("7. Enroll student into course");
    Console.WriteLine("8. Assign grade to student");
    Console.WriteLine("9. Update student information");
    Console.WriteLine("10. Delete a course");
    Console.WriteLine("11. Display enrollment report");
    Console.WriteLine("0. Exit");
    Console.WriteLine("------------------------------------");
    Console.Write("Select an option: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1": // Display all departments
            Console.Clear();
            Console.WriteLine("=== All Departments ===");
            var deptResult = departmentService.GetAllDepartments();
            if (deptResult.IsSuccess && deptResult.Data != null)
            {
                foreach (var d in deptResult.Data)
                {
                    Console.WriteLine($"{d.DepartmentId} - {d.Name} - {d.Description}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {deptResult.Message}");
            }
            Pause();
            break;

        case "2": // Display all students
            Console.Clear();
            Console.WriteLine("=== All Students ===");
            var studentResult = studentService.GetAllStudents();
            if (studentResult.IsSuccess && studentResult.Data != null)
            {
                foreach (var s in studentResult.Data)
                {
                    Console.WriteLine($"{s.StudentId} - {s.StudentCode} - {s.FullName} - {s.Email} - Dept:{s.DepartmentId}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {studentResult.Message}");
            }
            Pause();
            break;

        case "3": // Display all courses
            Console.Clear();
            Console.WriteLine("=== All Courses ===");
            var courseResult = courseService.GetAllCourses();
            if (courseResult.IsSuccess && courseResult.Data != null)
            {
                foreach (var c in courseResult.Data)
                {
                    Console.WriteLine($"{c.CourseId} - {c.CourseCode} - {c.Title} - Credits:{c.Credits} - Status:{c.Status}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {courseResult.Message}");
            }
            Pause();
            break;

        case "4": // Add new department
            Console.Clear();
            Console.WriteLine("=== Add New Department ===");
            Console.Write("Department Name: ");
            var deptName = Console.ReadLine();
            Console.Write("Description (optional): ");
            var deptDesc = Console.ReadLine();

            var newDept = new Department
            {
                Name = deptName!,
                Description = deptDesc
            };

            var addDeptResult = departmentService.AddDepartment(newDept);
            Console.WriteLine(addDeptResult.IsSuccess
                ? $"✓ Success: {addDeptResult.Message}"
                : $"✗ Failed: {addDeptResult.Message}");
            Pause();
            break;

        case "5": // Add new student
            Console.Clear();
            Console.WriteLine("=== Add New Student ===");
            Console.Write("Student Code: ");
            var stuCode = Console.ReadLine();
            Console.Write("Full Name: ");
            var stuName = Console.ReadLine();
            Console.Write("Email: ");
            var stuEmail = Console.ReadLine();
            Console.Write("Department ID: ");
            int stuDeptId = int.Parse(Console.ReadLine()!);

            var newStudent = new Student
            {
                StudentCode = stuCode!,
                FullName = stuName!,
                Email = stuEmail!,
                DepartmentId = stuDeptId
            };

            var addStuResult = studentService.AddStudent(newStudent);
            Console.WriteLine(addStuResult.IsSuccess
                ? $"✓ Success: {addStuResult.Message}"
                : $"✗ Failed: {addStuResult.Message}");
            Pause();
            break;

        case "6": // Add new course
            Console.Clear();
            Console.WriteLine("=== Add New Course ===");
            Console.Write("Course Code: ");
            var courseCode = Console.ReadLine();
            Console.Write("Title: ");
            var courseTitle = Console.ReadLine();
            Console.Write("Credits (1-6): ");
            int credits = int.Parse(Console.ReadLine()!);
            Console.Write("Department ID: ");
            int courseDeptId = int.Parse(Console.ReadLine()!);

            var newCourse = new Course
            {
                CourseCode = courseCode!,
                Title = courseTitle!,
                Credits = credits,
                DepartmentId = courseDeptId,
                Status = CourseStatus.Active
            };

            var addCourseResult = courseService.AddCourse(newCourse);
            Console.WriteLine(addCourseResult.IsSuccess
                ? $"✓ Success: {addCourseResult.Message}"
                : $"✗ Failed: {addCourseResult.Message}");
            Pause();
            break;

        case "7": // Enroll student
            Console.Clear();
            Console.WriteLine("=== Enroll Student into Course ===");
            Console.Write("Student ID: ");
            int enrollStudentId = int.Parse(Console.ReadLine()!);
            Console.Write("Course ID: ");
            int enrollCourseId = int.Parse(Console.ReadLine()!);

            var enrollResult = enrollmentService.EnrollStudent(enrollStudentId, enrollCourseId, DateTime.Now);
            Console.WriteLine(enrollResult.IsSuccess
                ? $"✓ Success: {enrollResult.Message}"
                : $"✗ Failed: {enrollResult.Message}");
            Pause();
            break;

        case "8": // Assign grade
            Console.Clear();
            Console.WriteLine("=== Assign Grade ===");
            Console.Write("Student ID: ");
            int gradeStudentId = int.Parse(Console.ReadLine()!);
            Console.Write("Course ID: ");
            int gradeCourseId = int.Parse(Console.ReadLine()!);
            Console.Write("Grade (0-10): ");
            decimal grade = decimal.Parse(Console.ReadLine()!);

            var gradeResult = enrollmentService.AssignGrade(gradeStudentId, gradeCourseId, grade);
            Console.WriteLine(gradeResult.IsSuccess
                ? $"✓ Success: {gradeResult.Message}"
                : $"✗ Failed: {gradeResult.Message}");

            if (gradeResult.IsSuccess)
            {
                Console.Write("\nFinalize this grade? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    var finalizeResult = enrollmentService.FinalizeGrade(gradeStudentId, gradeCourseId);
                    Console.WriteLine(finalizeResult.IsSuccess
                        ? $"✓ Grade finalized: {finalizeResult.Message}"
                        : $"✗ Failed to finalize: {finalizeResult.Message}");
                }
            }
            Pause();
            break;

        case "9": // Update student
            Console.Clear();
            Console.WriteLine("=== Update Student Information ===");
            Console.Write("Student ID: ");
            int updateId = int.Parse(Console.ReadLine()!);

            var existingStudent = studentService.GetStudentById(updateId);
            if (!existingStudent.IsSuccess || existingStudent.Data == null)
            {
                Console.WriteLine($"✗ Failed: {existingStudent.Message}");
                Pause();
                break;
            }

            Console.WriteLine($"Current Info: {existingStudent.Data.FullName} - {existingStudent.Data.Email}");
            Console.Write("New Full Name: ");
            var newName = Console.ReadLine();
            Console.Write("New Email: ");
            var newEmail = Console.ReadLine();

            existingStudent.Data.FullName = string.IsNullOrWhiteSpace(newName) ? existingStudent.Data.FullName : newName;
            existingStudent.Data.Email = string.IsNullOrWhiteSpace(newEmail) ? existingStudent.Data.Email : newEmail;

            var updateResult = studentService.UpdateStudent(existingStudent.Data);
            Console.WriteLine(updateResult.IsSuccess
                ? $"✓ Success: {updateResult.Message}"
                : $"✗ Failed: {updateResult.Message}");
            Pause();
            break;

        case "10": // Delete course
            Console.Clear();
            Console.WriteLine("=== Delete Course ===");
            Console.Write("Course ID to delete: ");
            int delId = int.Parse(Console.ReadLine()!);

            var deleteResult = courseService.DeleteCourse(delId);
            Console.WriteLine(deleteResult.IsSuccess
                ? $"✓ Success: {deleteResult.Message}"
                : $"✗ Failed: {deleteResult.Message}");
            Pause();
            break;

        case "11": // Enrollment report
            Console.Clear();
            Console.WriteLine("=== Enrollment Report ===");

            var enrollments = enrollmentService.GetAllEnrollments();
            if (enrollments.IsSuccess && enrollments.Data != null)
            {
                var students = studentService.GetAllStudents().Data;
                var courses = courseService.GetAllCourses().Data;

                Console.WriteLine($"{"Student",-20} | {"Course",-25} | {"Enroll Date",-12} | {"Grade",-6} | Finalized");
                Console.WriteLine(new string('-', 85));

                foreach (var e in enrollments.Data)
                {
                    var student = students?.FirstOrDefault(s => s.StudentId == e.StudentId);
                    var course = courses?.FirstOrDefault(c => c.CourseId == e.CourseId);

                    Console.WriteLine(
                        $"{student?.FullName,-20} | {course?.Title,-25} | {e.EnrollDate:yyyy-MM-dd} | {(e.Grade?.ToString("F1") ?? "N/A"),-6} | {(e.IsGradeFinalized ? "Yes" : "No")}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {enrollments.Message}");
            }
            Pause();
            break;

        case "0": // Exit
            Console.WriteLine("Goodbye!");
            return;

        default:
            Console.WriteLine("Invalid option!");
            Pause();
            break;
    }
}

static void Pause()
{
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
