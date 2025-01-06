using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using Backend.Models;
using Backend.Models.Communs;
using Backend.Models.Entities;
using Backend.Interfaces;

namespace Backend.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>, IAppDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;

        }

        // DbSets for domain entities
        public DbSet<UserPreferences> UserPreferences { get; set; } = default!;
        public DbSet<SSHHostConfig> SSHHostConfigs { get; set; } = default!;
        public DbSet<SSHSession> SSHSessions { get; set; } = default!;
        public DbSet<SSHCommand> SSHCommands { get; set; } = default!;
        public DbSet<AIConversation> AIConversations { get; set; } = default!;
        public DbSet<AIMessage> AIMessages { get; set; } = default!;

        public override DbSet<T> Set<T>()
        {
            return base.Set<T>();
        }

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            var entries = ChangeTracker.Entries<AuditableEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Created = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userName;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModified = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = userName;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and cascading behaviors
            builder.Entity<AppUser>()
                .HasMany(u => u.SSHHostConfigs)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppUser>()
                .HasOne(u => u.UserPreferences)
                .WithOne(up => up.User)
                .HasForeignKey<UserPreferences>(up => up.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPreferences>()
                .HasOne(up => up.DefaultSSHHost)
                .WithMany(c => c.DefaultPrefs)
                .HasForeignKey(up => up.DefaultSSHHostId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SSHSession>()
                .HasOne(s => s.User)
                .WithMany(u => u.SSHSessions)
                .HasForeignKey(s => s.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SSHSession>()
                .HasOne(s => s.SSHHostConfig)
                .WithMany(c => c.SSHSessions)
                .HasForeignKey(s => s.SSHHostConfigId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SSHCommand>()
                .HasOne(c => c.SSHSession)
                .WithMany(s => s.SSHCommands)
                .HasForeignKey(c => c.SSHSessionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AIConversation>()
                .HasOne(a => a.User)
                .WithMany(u => u.AIConversations)
                .HasForeignKey(a => a.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AIConversation>()
                .HasOne(a => a.LinkedSSHSession)
                .WithMany(s => s.AIConversations)
                .HasForeignKey(a => a.SSHSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AIMessage>()
                .HasOne(m => m.AIConversation)
                .WithMany(a => a.AIMessages)
                .HasForeignKey(m => m.AIConversationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Identity tables to use string keys and rename table names
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable(name: "AspNetUsers");
            });

            builder.Entity<AppRole>(entity =>
            {
                entity.ToTable(name: "AspNetRoles");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("AspNetUserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("AspNetUserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("AspNetUserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("AspNetRoleClaims");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("AspNetUserTokens");
            });
        }
    }
}
