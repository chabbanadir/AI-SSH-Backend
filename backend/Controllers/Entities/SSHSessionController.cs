// Controllers/SSHSessionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Backend.Interfaces;
using Backend.Context;
using Backend.Models.Entities;

namespace Backend.Controllers.Entities{
[ApiController]
    [Route("api/[controller]")]
    public class SSHSessionController : ControllerBase
    {
        private readonly ISSHService _sshService;
        private readonly AppDbContext _context;
        private readonly IBulkInsertService _bulkInsertService;

        public SSHSessionController(ISSHService sshService, AppDbContext context, IBulkInsertService bulkInsertService)
        {
            _sshService = sshService;
            _context = context;
            _bulkInsertService = bulkInsertService;
        }

        // POST: api/SSHSession
        [HttpPost]
        public async Task<ActionResult<SSHSession>> StartSession(StartSessionRequest request, CancellationToken cancellationToken)
        {
            var config = await _context.SSHHostConfigs.FindAsync(new object[] { request.SSHHostConfigId }, cancellationToken);
            if (config == null)
            {
                return BadRequest("Invalid SSH Host Configuration.");
            }

            var session = await _sshService.StartSessionAsync(config, cancellationToken);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        // GET: api/SSHSession/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SSHSession>> GetSession(string id, CancellationToken cancellationToken)
        {
            var session = await _context.SSHSessions
                .Include(s => s.SSHHostConfig)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }

        // POST: api/SSHSession/{id}/ExecuteCommand
        [HttpPost("{id}/ExecuteCommand")]
        public async Task<IActionResult> ExecuteCommand(string id, ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _sshService.ExecuteCommandAsync(id, request.Command, cancellationToken);
                return Ok(new { Message = "Command executed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/SSHSession/{id}/End
        [HttpPost("{id}/End")]
        public async Task<IActionResult> EndSession(string id, CancellationToken cancellationToken)
        {
            try
            {
                await _sshService.EndSessionAsync(id, cancellationToken);

                // Retrieve SSHCommands related to this session
                var commands = await _context.SSHCommands.Where(c => c.LinkedSSHSessionId == id).ToListAsync(cancellationToken);

                if (commands.Any())
                {
                    await _bulkInsertService.BulkInsertSSHCommandsAsync(commands, cancellationToken);
                    _context.SSHCommands.RemoveRange(commands);
                }

                // Similarly, handle AIConversations and AIMessages if applicable

                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new { Message = "Session ended and data bulk inserted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (implement logging as needed)
                return StatusCode(500, "An error occurred while ending the session.");
            }
        }
    }

    // DTOs for requests
    public class StartSessionRequest
    {
        public string SSHHostConfigId { get; set; }
    }

    public class ExecuteCommandRequest
    {
        public string Command { get; set; }
    }
}