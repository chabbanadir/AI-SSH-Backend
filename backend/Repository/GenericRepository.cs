using Microsoft.EntityFrameworkCore;
using Backend.Context;
using Backend.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IAppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(IAppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>(); // In your real code, you need `IAppDbContext` to expose `DbSet<T> Set<T>()`.
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}