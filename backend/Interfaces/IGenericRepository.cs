using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Backend.Models.Entities.AI;
namespace Backend.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T?>> GetAllAsync();
        Task<T?> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> SaveChangesAsync();
      
    }
}
