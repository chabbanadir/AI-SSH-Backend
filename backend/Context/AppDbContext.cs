using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using Backend.Interfaces;
namespace Backend.Context{

   // If you are using Identity, you typically inherit from IdentityDbContext:
    public class AppDbContext 
        : IdentityDbContext<AppUser, IdentityRole, string>
        , IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets for each entity in your domain:
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public override DbSet<T> Set<T>()
        {
            return base.Set<T>();
        }
        // If you have custom IdentityUser or IdentityRole classes, 
        // you would define them here as well, if needed.

        // Implement the interface methods.
        public override int SaveChanges()
        {
            // You can add extra logic here (e.g., auditing) if needed
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // You can add extra logic here (e.g., auditing) if needed
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // For IdentityDbContext-based projects, remember to call the base:
            base.OnModelCreating(builder);

            // Configure entity relationships, constraints, etc., e.g.:
            // builder.Entity<Product>(entity =>
            // {
            //     entity.HasKey(e => e.Id);
            //     ...
            // });
        }
    }
}