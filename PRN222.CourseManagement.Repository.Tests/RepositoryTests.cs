using CourseManagement.Models;
using CourseManagement.Repositories.Implementations;
using NUnit.Framework;
using System;
using System.Linq;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class RepositoryTests
{
    // 6.1 GetAll returns data
    [Test]
    public void GetAll_ReturnsData()
    {
        using var context = TestHelper.CreateInMemoryContext();

        context.Departments.Add(new Department { DepartmentId = 1, Name = "SE" });
        context.SaveChanges();

        var repo = new GenericRepository<Department>(context);

        var result = repo.GetAll();

        Assert.That(result, Is.Not.Empty);
    }

    // 6.2 Add inserts a new entity
    [Test]
    public void Add_InsertsStudent()
    {
        using var context = TestHelper.CreateInMemoryContext();

        context.Departments.Add(new Department { DepartmentId = 1, Name = "SE" });
        context.SaveChanges();

        var repo = new GenericRepository<Student>(context);

        repo.Add(new Student
        {
            StudentId = 1,
            StudentCode = "S1",
            FullName = "A",
            Email = "a@test.com",
            DepartmentId = 1
        });
        context.SaveChanges();

        Assert.That(context.Students.Count(), Is.EqualTo(1));
    }

    // 6.3 GetById returns correct entity
    [Test]
    public void GetById_ReturnsCorrectStudent()
    {
        using var context = TestHelper.CreateInMemoryContext();

        context.Departments.Add(new Department { DepartmentId = 1, Name = "SE" });
        context.Students.Add(new Student
        {
            StudentId = 1,
            StudentCode = "S1",
            FullName = "A",
            Email = "a@test.com",
            DepartmentId = 1
        });
        context.SaveChanges();

        var repo = new GenericRepository<Student>(context);
        var student = repo.GetById(1);

        Assert.That(student, Is.Not.Null);
        Assert.That(student!.FullName, Is.EqualTo("A"));
    }

    // 6.4 Delete removes entity
    [Test]
    public void Delete_RemovesStudent()
    {
        using var context = TestHelper.CreateInMemoryContext();

        context.Departments.Add(new Department { DepartmentId = 1, Name = "SE" });
        context.Students.Add(new Student
        {
            StudentId = 1,
            StudentCode = "S1",
            FullName = "A",
            Email = "a@test.com",
            DepartmentId = 1
        });
        context.SaveChanges();

        var repo = new GenericRepository<Student>(context);
        repo.Delete(1);
        context.SaveChanges();

        Assert.That(context.Students.Count(), Is.EqualTo(0));
    }

    // 6.5 UnitOfWork.Save persists multiple entities
    [Test]
    public void UnitOfWork_Save_PersistsEntities()
    {
        using var context = TestHelper.CreateInMemoryContext();
        var uow = new UnitOfWork(context);

        uow.Departments.Add(new Department { DepartmentId = 1, Name = "IT" });
        uow.Students.Add(new Student
        {
            StudentId = 1,
            StudentCode = "S1",
            FullName = "A",
            Email = "a@test.com",
            DepartmentId = 1
        });

        uow.Save();

        Assert.That(context.Departments.Count(), Is.EqualTo(1));
        Assert.That(context.Students.Count(), Is.EqualTo(1));
    }

    // 6.6 Bonus: N-N Enrollment
    [Test]
    public void CreateEnrollment_Works()
    {
        using var context = TestHelper.CreateInMemoryContext();

        context.Departments.Add(new Department { DepartmentId = 1, Name = "IT" });
        context.Students.Add(new Student
        {
            StudentId = 1,
            StudentCode = "S1",
            FullName = "A",
            Email = "a@test.com",
            DepartmentId = 1
        });
        context.Courses.Add(new Course
        {
            CourseId = 1,
            CourseCode = "C01",
            Title = "C#",
            Credits = 3,
            DepartmentId = 1
        });
        context.SaveChanges();

        var uow = new UnitOfWork(context);
        uow.Enrollments.Add(new Enrollment
        {
            StudentId = 1,
            CourseId = 1,
            EnrollDate = DateTime.Now,
            Grade = null
        });
        uow.Save();

        Assert.That(context.Enrollments.Count(), Is.EqualTo(1));
    }
}
