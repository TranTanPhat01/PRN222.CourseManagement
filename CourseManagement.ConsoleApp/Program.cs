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
    throw new InvalidOperationException("Missing ConnectionStrings:CourseManagementDB in appsettings.json");

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
        case "1": DisplayDepartments(departmentService); break;
        case "2": DisplayStudents(studentService); break;
        case "3": DisplayCourses(courseService); break;
        case "4": AddDepartment(departmentService); break;
        case "5": AddStudent(studentService); break;
        case "6": AddCourse(courseService); break;
        case "7": EnrollStudent(enrollmentService); break;
        case "8": AssignGrade(enrollmentService); break;
        case "9": UpdateStudent(studentService); break;
        case "10": DeleteCourse(courseService); break;
        case "11": DisplayEnrollmentReport(enrollmentService, studentService, courseService); break;
        case "0": Console.WriteLine("Goodbye!"); return;
        default: Console.WriteLine("Invalid option!"); Pause(); break;
    }
}

static void DisplayDepartments(IDepartmentService service)
{
    Console.Clear();
    Console.WriteLine("=== All Departments ===");
    var result = service.GetAllDepartments();
    if (result.IsSuccess && result.Data != null)
    {
        foreach (var d in result.Data) Console.WriteLine($"{d.DepartmentId} - {d.Name} - {d.Description}");
    }
    else Console.WriteLine($"Error: {result.Message}");
    Pause();
}

static void DisplayStudents(IStudentService service)
{
    Console.Clear();
    Console.WriteLine("=== All Students ===");
    var result = service.GetAllStudents();
    if (result.IsSuccess && result.Data != null)
    {
        foreach (var s in result.Data) Console.WriteLine($"{s.StudentId} - {s.StudentCode} - {s.FullName} - {s.Email} - Dept:{s.DepartmentId}");
    }
    else Console.WriteLine($"Error: {result.Message}");
    Pause();
}

static void DisplayCourses(ICourseService service)
{
    Console.Clear();
    Console.WriteLine("=== All Courses ===");
    var result = service.GetAllCourses();
    if (result.IsSuccess && result.Data != null)
    {
        foreach (var c in result.Data) Console.WriteLine($"{c.CourseId} - {c.CourseCode} - {c.Title} - Credits:{c.Credits} - Status:{c.Status}");
    }
    else Console.WriteLine($"Error: {result.Message}");
    Pause();
}

static void AddDepartment(IDepartmentService service)
{
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

    var addDeptResult = service.AddDepartment(newDept);
    Console.WriteLine(addDeptResult.IsSuccess
        ? $"✓ Success: {addDeptResult.Message}"
        : $"✗ Failed: {addDeptResult.Message}");
    Pause();
}

static void AddStudent(IStudentService service)
{
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

    var addStuResult = service.AddStudent(newStudent);
    Console.WriteLine(addStuResult.IsSuccess
        ? $"✓ Success: {addStuResult.Message}"
        : $"✗ Failed: {addStuResult.Message}");
    Pause();
}

static void AddCourse(ICourseService service)
{
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

    var addCourseResult = service.AddCourse(newCourse);
    Console.WriteLine(addCourseResult.IsSuccess
        ? $"✓ Success: {addCourseResult.Message}"
        : $"✗ Failed: {addCourseResult.Message}");
    Pause();
}

static void EnrollStudent(IEnrollmentService service)
{
    Console.Clear();
    Console.WriteLine("=== Enroll Student into Course ===");
    Console.Write("Student ID: ");
    int enrollStudentId = int.Parse(Console.ReadLine()!);
    Console.Write("Course ID: ");
    int enrollCourseId = int.Parse(Console.ReadLine()!);

    var enrollResult = service.EnrollStudent(enrollStudentId, enrollCourseId, DateTime.Now);
    Console.WriteLine(enrollResult.IsSuccess
        ? $"✓ Success: {enrollResult.Message}"
        : $"✗ Failed: {enrollResult.Message}");
    Pause();
}

static void AssignGrade(IEnrollmentService service)
{
    Console.Clear();
    Console.WriteLine("=== Assign Grade ===");
    Console.Write("Student ID: ");
    int gradeStudentId = int.Parse(Console.ReadLine()!);
    Console.Write("Course ID: ");
    int gradeCourseId = int.Parse(Console.ReadLine()!);
    Console.Write("Grade (0-10): ");
    decimal grade = decimal.Parse(Console.ReadLine()!);

    var gradeResult = service.AssignGrade(gradeStudentId, gradeCourseId, grade);
    Console.WriteLine(gradeResult.IsSuccess
        ? $"✓ Success: {gradeResult.Message}"
        : $"✗ Failed: {gradeResult.Message}");

    if (gradeResult.IsSuccess)
    {
        Console.Write("\nFinalize this grade? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            var finalizeResult = service.FinalizeGrade(gradeStudentId, gradeCourseId);
            Console.WriteLine(finalizeResult.IsSuccess
                ? $"✓ Grade finalized: {finalizeResult.Message}"
                : $"✗ Failed to finalize: {finalizeResult.Message}");
        }
    }
    Pause();
}

static void UpdateStudent(IStudentService service)
{
    Console.Clear();
    Console.WriteLine("=== Update Student Information ===");
    Console.Write("Student ID: ");
    int updateId = int.Parse(Console.ReadLine()!);

    var existingStudent = service.GetStudentById(updateId);
    if (!existingStudent.IsSuccess || existingStudent.Data == null)
    {
        Console.WriteLine($"✗ Failed: {existingStudent.Message}");
        Pause();
        return;
    }

    Console.WriteLine($"Current Info: {existingStudent.Data.FullName} - {existingStudent.Data.Email}");
    Console.Write("New Full Name: ");
    var newName = Console.ReadLine();
    Console.Write("New Email: ");
    var newEmail = Console.ReadLine();

    existingStudent.Data.FullName = string.IsNullOrWhiteSpace(newName) ? existingStudent.Data.FullName : newName;
    existingStudent.Data.Email = string.IsNullOrWhiteSpace(newEmail) ? existingStudent.Data.Email : newEmail;

    var updateResult = service.UpdateStudent(existingStudent.Data);
    Console.WriteLine(updateResult.IsSuccess
        ? $"✓ Success: {updateResult.Message}"
        : $"✗ Failed: {updateResult.Message}");
    Pause();
}

static void DeleteCourse(ICourseService service)
{
    Console.Clear();
    Console.WriteLine("=== Delete Course ===");
    Console.Write("Course ID to delete: ");
    int delId = int.Parse(Console.ReadLine()!);

    var deleteResult = service.DeleteCourse(delId);
    Console.WriteLine(deleteResult.IsSuccess
        ? $"✓ Success: {deleteResult.Message}"
        : $"✗ Failed: {deleteResult.Message}");
    Pause();
}

static void DisplayEnrollmentReport(IEnrollmentService enrollService, IStudentService stuService, ICourseService couService)
{
    Console.Clear();
    Console.WriteLine("=== Enrollment Report ===");

    var enrollments = enrollService.GetAllEnrollments();
    if (enrollments.IsSuccess && enrollments.Data != null)
    {
        var students = stuService.GetAllStudents().Data;
        var courses = couService.GetAllCourses().Data;

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
}

static void Pause()
{
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
