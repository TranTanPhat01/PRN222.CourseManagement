using CourseManagement.Models;
using CourseManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseManagement.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CourseManagementContext _context;
        private IDbContextTransaction? _transaction;

        public IGenericRepository<Student> Students { get; }
        public IGenericRepository<Course> Courses { get; }
        public IGenericRepository<Department> Departments { get; }
        public IGenericRepository<Enrollment> Enrollments { get; }

        public UnitOfWork(CourseManagementContext context)
        {
            _context = context;

            Students = new GenericRepository<Student>(_context);
            Courses = new GenericRepository<Course>(_context);
            Departments = new GenericRepository<Department>(_context);
            Enrollments = new GenericRepository<Enrollment>(_context);
        }

        public int Save() => _context.SaveChanges();

        public IDbContextTransaction BeginTransaction()
        {
            try
            {
                _transaction = _context.Database.BeginTransaction();
                return _transaction;
            }
            catch
            {
                // InMemory provider doesn't support transactions
                // Return null and handle gracefully
                return null!;
            }
        }

        public void Commit()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
