using Microsoft.EntityFrameworkCore;
using RPMS.DAL.Data;
using RPMS.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RPMS.DAL.Repositories.Implements
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly RPMSContext _context;
        internal DbSet<T> _dbSet;

        public GenericRepository(RPMSContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet.Where(expression);
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet.Where(expression);
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            return await query.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity).ConfigureAwait(false);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities).ConfigureAwait(false);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression).ConfigureAwait(false);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return await _dbSet.CountAsync().ConfigureAwait(false);
            return await _dbSet.CountAsync(expression).ConfigureAwait(false);
        }
    }
}