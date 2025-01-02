// Controllers/SSHHostConfigController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//using Backend.Interfaces;
using Backend.Context;
using Backend.Models.Entities;

namespace Backend.Controllers.Entities{
    [ApiController]
    [Route("api/[controller]")]
    public class SSHHostConfigController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SSHHostConfigController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SSHHostConfig
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SSHHostConfig>>> GetSSHHostConfigs(CancellationToken cancellationToken)
        {
            return await _context.SSHHostConfigs.ToListAsync(cancellationToken);
        }

        // GET: api/SSHHostConfig/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SSHHostConfig>> GetSSHHostConfig(string id, CancellationToken cancellationToken)
        {
            var config = await _context.SSHHostConfigs.FindAsync(new object[] { id }, cancellationToken);

            if (config == null)
            {
                return NotFound();
            }

            return config;
        }

        // POST: api/SSHHostConfig
        [HttpPost]
        public async Task<ActionResult<SSHHostConfig>> CreateSSHHostConfig(SSHHostConfig config, CancellationToken cancellationToken)
        {
            config.Id = Guid.NewGuid().ToString();
            config.SSHDefaultConfig = false; // Ensure default value

            _context.SSHHostConfigs.Add(config);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetSSHHostConfig), new { id = config.Id }, config);
        }

        // PUT: api/SSHHostConfig/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSSHHostConfig(string id, SSHHostConfig updatedConfig, CancellationToken cancellationToken)
        {
            if (id != updatedConfig.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedConfig).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SSHHostConfigExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/SSHHostConfig/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSSHHostConfig(string id, CancellationToken cancellationToken)
        {
            var config = await _context.SSHHostConfigs.FindAsync(new object[] { id }, cancellationToken);
            if (config == null)
            {
                return NotFound();
            }

            _context.SSHHostConfigs.Remove(config);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        private bool SSHHostConfigExists(string id)
        {
            return _context.SSHHostConfigs.Any(e => e.Id == id);
        }
    }
}