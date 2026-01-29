using CourseManagement.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseManagement.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Student> Students { get; }
        IGenericRepository<Course> Courses { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Enrollment> Enrollments { get; }

        int Save();
        IDbContextTransaction BeginTransaction();
        void Commit();
        void Rollback();
    }
}
