using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

using Backend.Models;
using Backend.Models.Entities;
namespace Backend.Interfaces
{
    public interface IAppDbContext
    {
        // DbSets for each entity in your domain:
    // DbSets
    public DbSet<AppUser> Users { get; set; } 
    public DbSet<UserPreferences> UserPreferences { get; set; } 
    public DbSet<SSHHostConfig> SSHHostConfigs { get; set; } 
    public DbSet<SSHSession> SSHSessions { get; set; } 
    public DbSet<SSHCommand> SSHCommands { get; set; } 
    public DbSet<AIConversation> AIConversations { get; set; } 
    public DbSet<AIMessage> AIMessages { get; set; } 


      DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    }
}
