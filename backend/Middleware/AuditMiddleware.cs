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
            // Proceed to the next middleware/component
            await _next(context);

            // After the request is processed, set audit fields
            var userName = context.User.Identity?.IsAuthenticated == true 
                ? context.User.Identity.Name 
                : "Anonymous";

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

            // Save changes if any audit fields were set
            await dbContext.SaveChangesAsync();
        }
    }
}
