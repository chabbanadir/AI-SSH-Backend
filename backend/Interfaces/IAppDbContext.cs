using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
namespace Backend.Interfaces
{
    public interface IAppDbContext
    {
        // DbSets for each entity in your domain:
        DbSet<AppUser> AppUsers { get; set; }
        DbSet<Todo> Todos { get; set; }

      DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    }
}
