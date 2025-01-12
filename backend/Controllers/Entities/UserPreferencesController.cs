// Controllers/UserPreferencesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//using Backend.Interfaces;
using Backend.Context;
using Backend.Models.Entities.SSH;

namespace Backend.Controllers.Entities{

    [ApiController]
    [Route("api/[controller]")]
    public class UserPreferencesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserPreferencesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserPreferences/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserPreferences>> GetUserPreferences(string userId, CancellationToken cancellationToken)
        {
            var preferences = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            if (preferences == null)
            {
                return NotFound();
            }

            return preferences;
        }

        // PUT: api/UserPreferences/{userId}
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserPreferences(string userId, UserPreferences updatedPreferences, CancellationToken cancellationToken)
        {
            if (userId != updatedPreferences.UserId)
            {
                return BadRequest();
            }

            var existingPreferences = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            if (existingPreferences == null)
            {
                return NotFound();
            }

            // Update fields
            existingPreferences.PreferenceToken = updatedPreferences.PreferenceToken;
            existingPreferences.IsDefault = updatedPreferences.IsDefault;
            // Update other preference fields as necessary

            _context.UserPreferences.Update(existingPreferences);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}