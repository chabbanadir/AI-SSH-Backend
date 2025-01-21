// Middleware/AuditMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models.Entities;
using Backend.Models.Communs;

using Backend.Context;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Bypass authentication for specific paths
            var excludedPaths = new[] { "/api/users/login", "/api/users/register" }; // Add more if needed
            if (excludedPaths.Contains(context.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context); // Skip the authentication check
                return;
            }

            // Check if the user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? false)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            // Proceed with the audit logic
            await _next(context);

            var userName = context.User.Identity.Name ?? "Anonymous";

            foreach (var entry in dbContext.ChangeTracker.Entries<AuditableEntity>())
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

            await dbContext.SaveChangesAsync();
        }


    }
}
