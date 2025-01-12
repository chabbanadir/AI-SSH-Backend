// Controllers/SSHHostConfigController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//using Backend.Interfaces;
using Backend.Interfaces;
using Backend.Models.Entities.SSH;

using Backend.Models.Dtos;

namespace Backend.Controllers.Entities
{
    [ApiController]
    [Route("api/[controller]")]
    public class SSHHostConfigController : ControllerBase
    {
        private readonly IGenericRepository<SSHHostConfig> _sshHostConfigRepo;

        public SSHHostConfigController(IGenericRepository<SSHHostConfig> sshHostConfigRepo)
        {
            _sshHostConfigRepo = sshHostConfigRepo;
        }

        // GET: api/SSHHostConfig
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SSHHostConfigDTO>>> GetSSHHostConfigs()
        {
            try
            {
                var configs = await _sshHostConfigRepo.GetAllAsync();

                // Map entities to DTOs
                var configDTOs = new List<SSHHostConfigDTO>();
                foreach (var config in configs)
                {
                    if (config != null)
                    {
                        configDTOs.Add(new SSHHostConfigDTO
                        {
                            Id = config.Id,
                            Hostname = config.Hostname,
                            Port = config.Port,
                            Username = config.Username,
                            AuthType = config.AuthType,
                            PasswordOrKeyPath = config.PasswordOrKeyPath,
                            UserId = config.UserId,
                            SSHDefaultConfig = config.SSHDefaultConfig
                        });
                    }
                }

                return Ok(configDTOs);
            }
            catch (Exception)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while retrieving SSH Host Configurations.");
            }
        }

        // GET: api/SSHHostConfig/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SSHHostConfigDTO>> GetSSHHostConfig(string id)
        {
            try
            {
                var config = await _sshHostConfigRepo.GetByIdAsync(id);
                if (config == null)
                {
                    return NotFound();
                }

                // Map entity to DTO
                var configDTO = new SSHHostConfigDTO
                {
                    Id = config.Id,
                    Hostname = config.Hostname,
                    Port = config.Port,
                    Username = config.Username,
                    AuthType = config.AuthType,
                    PasswordOrKeyPath = config.PasswordOrKeyPath,
                    UserId = config.UserId,
                    SSHDefaultConfig = config.SSHDefaultConfig
                };

                return Ok(configDTO);
            }
            catch (Exception)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while retrieving the SSH Host Configuration.");
            }
        }

        // POST: api/SSHHostConfig
        [HttpPost]
        public async Task<ActionResult<SSHHostConfigDTO>> CreateSSHHostConfig([FromBody] CreateSSHHostConfigDTO createDTO)
        {
            try
            {
                // Map DTO to entity
                var config = new SSHHostConfig
                {
                    Hostname = createDTO.Hostname,
                    Port = createDTO.Port,
                    Username = createDTO.Username,
                    AuthType = createDTO.AuthType,
                    PasswordOrKeyPath = createDTO.PasswordOrKeyPath,
                    UserId = createDTO.UserId,
                    SSHDefaultConfig = createDTO.SSHDefaultConfig
                };

                var addedConfig = await _sshHostConfigRepo.AddAsync(config);
                var saveResult = await _sshHostConfigRepo.SaveChangesAsync();
                if (!saveResult)
                {
                    return StatusCode(500, "An error occurred while saving the SSH Host Configuration.");
                }

                // Map added entity to DTO
                var configDTO = new SSHHostConfigDTO
                {
                    Id = addedConfig.Id,
                    Hostname = addedConfig.Hostname,
                    Port = addedConfig.Port,
                    Username = addedConfig.Username,
                    AuthType = addedConfig.AuthType,
                    PasswordOrKeyPath = addedConfig.PasswordOrKeyPath,
                    UserId = addedConfig.UserId,
                    SSHDefaultConfig = addedConfig.SSHDefaultConfig
                };

                return CreatedAtAction(nameof(GetSSHHostConfig), new { id = configDTO.Id }, configDTO);
            }
            catch (Exception)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while creating the SSH Host Configuration.");
            }
        }

        // PUT: api/SSHHostConfig/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSSHHostConfig(string id, [FromBody] SSHHostConfigDTO updateDTO)
        {
            if (id != updateDTO.Id)
            {
                return BadRequest("ID mismatch.");
            }

            try
            {
                // Retrieve existing configuration
                var existingConfig = await _sshHostConfigRepo.GetByIdAsync(id);
                if (existingConfig == null)
                {
                    return NotFound();
                }

                // Update properties
                existingConfig.Hostname = updateDTO.Hostname;
                existingConfig.Port = updateDTO.Port;
                existingConfig.Username = updateDTO.Username;
                existingConfig.AuthType = updateDTO.AuthType;
                existingConfig.PasswordOrKeyPath = updateDTO.PasswordOrKeyPath;
                existingConfig.UserId = updateDTO.UserId;
                existingConfig.SSHDefaultConfig = updateDTO.SSHDefaultConfig;

                await _sshHostConfigRepo.UpdateAsync(existingConfig);
                var saveResult = await _sshHostConfigRepo.SaveChangesAsync();
                if (!saveResult)
                {
                    return StatusCode(500, "An error occurred while updating the SSH Host Configuration.");
                }

                return NoContent();
            }
            catch (Exception)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while updating the SSH Host Configuration.");
            }
        }

        // DELETE: api/SSHHostConfig/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSSHHostConfig(string id)
        {
            try
            {
                var config = await _sshHostConfigRepo.GetByIdAsync(id);
                if (config == null)
                {
                    return NotFound();
                }

                await _sshHostConfigRepo.DeleteAsync(config);
                var saveResult = await _sshHostConfigRepo.SaveChangesAsync();
                if (!saveResult)
                {
                    return StatusCode(500, "An error occurred while deleting the SSH Host Configuration.");
                }

                return NoContent();
            }
            catch (Exception)
            {
                // Handle exceptions and return appropriate error responses
                return StatusCode(500, "An error occurred while deleting the SSH Host Configuration.");
            }
        }
    }
}