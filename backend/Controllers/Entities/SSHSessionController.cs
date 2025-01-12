// Controllers/SSHSessionController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Backend.Interfaces;
using Backend.Models.Dtos;
using Backend.Models.Entities.SSH;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers.Entities
{
    [ApiController]
    [Route("api/[controller]")]
    public class SSHSessionController : ControllerBase
    {
        private readonly ILogger<SSHSessionController> _logger;
        private readonly ISSHService _sshService;
        private readonly IGenericRepository<SSHHostConfig> _sshHostConfigRepo;
        private readonly IGenericRepository<SSHSession> _sshSessionRepo;

        public SSHSessionController(
            ISSHService sshService,
            IGenericRepository<SSHHostConfig> sshHostConfigRepo,
            IGenericRepository<SSHSession> sshSessionRepo,
            ILogger<SSHSessionController> logger)
        {
            _sshService = sshService;
            _sshHostConfigRepo = sshHostConfigRepo;
            _logger = logger;
            _sshSessionRepo = sshSessionRepo;
        }

        // POST: api/SSHSession
        [HttpPost]
        public async Task<ActionResult<SSHSession>> StartSession(StartSessionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve SSHHostConfig using the generic repository


                var config = await _sshHostConfigRepo.GetByIdAsync(request.SSHHostConfigId);
                if (config == null)
                {
                    return BadRequest("Invalid SSH Host Configuration.");
                }
                _logger.LogInformation("SSH config found");
                var session = await _sshService.StartSessionAsync(config, cancellationToken);
                _logger.LogInformation("_sshService executed");
                return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while starting the SSH session." );
            }
        }
        


        // GET: api/SSHSession/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SSHSession>> GetSession(string id, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve SSHSession using the generic repository
                var session = await _sshSessionRepo.GetByIdAsync(id);
                if (session == null)
                {
                    return NotFound();
                }

                return session;
            }
            catch 
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while retrieving the SSH session.");
            }
        }

        // POST: api/SSHSession/{id}/ExecuteCommand
        [HttpPost("{id}/ExecuteCommand")]
        public async Task<IActionResult> ExecuteCommand(string id, ExecuteCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _sshService.ExecuteCommandAsync(id, request.Command, cancellationToken);
                return Ok(new { Output = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch 
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while executing the command.");
            }
        }

        // POST: api/SSHSession/{id}/End
        [HttpPost("{id}/End")]
        public async Task<IActionResult> EndSession(string id, CancellationToken cancellationToken)
        {
            try
            {
                await _sshService.EndSessionAsync(id, cancellationToken);
                return Ok(new { Message = "Session ended and data bulk inserted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch 
            {
                // Handle exceptions and return appropriate error responses
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