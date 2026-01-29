using CourseManagement.Models;
using CourseManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CourseManagement.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly CourseManagementContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(CourseManagementContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
            => _dbSet.ToList();

        public T? GetById(object id)
            => _dbSet.Find(id);

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
            => _dbSet.Where(predicate).ToList();

        public void Add(T entity)
            => _dbSet.Add(entity);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public void Delete(T entity)
        {
            if (entity != null)
                _dbSet.Remove(entity);
        }
        }
    }