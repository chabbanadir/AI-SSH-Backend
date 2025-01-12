using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Backend.Interfaces;
using Backend.Models.Entities.SSH;

namespace Backend.Services.SSH
{
    public class SSHService : ISSHService
    {
        private readonly ISSHSessionManager _sessionManager;
        private readonly ILogger<SSHService> _logger;
        private readonly IGenericRepository<SSHSession> _sshSessionRepo;
        private readonly IGenericRepository<SSHCommand> _sshCommandRepo;

        public SSHService(
            ISSHSessionManager sessionManager,
            IGenericRepository<SSHSession> sshSessionRepo,
            IGenericRepository<SSHCommand> sshCommandRepo,
            ILogger<SSHService> logger)
        {
            _sessionManager = sessionManager;
            _sshSessionRepo = sshSessionRepo;
            _sshCommandRepo = sshCommandRepo;
            _logger = logger;
        }

        /// <summary>
        /// Starts an SSH session by establishing a connection to the specified host.
        /// </summary>
        public async Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken)
        {
            SshClient client = null;
            string sessionId = null;

            try
            {
                _logger.LogInformation("Attempting to connect SSH to {Hostname}:{Port} as {Username}", config.Hostname, config.Port, config.Username);

                // Establish SSH connection
                client = new SshClient(config.Hostname, config.Port, config.Username, config.PasswordOrKeyPath);
                client.Connect();
                _logger.LogInformation("SSH connected to {Hostname}:{Port}", config.Hostname, config.Port);

                // Retrieve the initial working directory
                string initialWorkingDirectory = null;
                try
                {
                    var command = client.CreateCommand("pwd");
                    initialWorkingDirectory = command.Execute().Trim();
                    _logger.LogInformation("Initial working directory: {InitialWorkingDirectory}", initialWorkingDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve the initial working directory.");
                }

                // Create a new session
                var session = new SSHSession
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionStartTime = DateTime.UtcNow,
                    UserId = config.UserId,
                    SSHHostConfigId = config.Id,
                    SSHHostConfig = config,
                    InitialWorkingDirectory = initialWorkingDirectory
                };
                sessionId = session.Id;

                // Save session to repository
                await _sshSessionRepo.AddAsync(session);
                await _sshSessionRepo.SaveChangesAsync();
                _logger.LogInformation("Session with ID: {SessionId} saved in repository.", sessionId);

                // Add session to active sessions
                if (!_sessionManager.ActiveSessions.TryAdd(sessionId, client))
                {
                    throw new Exception("Failed to add SSH session to active sessions.");
                }

                _sessionManager.SessionCommands.TryAdd(sessionId, new List<SSHCommand>());
                _logger.LogInformation("Session with ID: {SessionId} successfully started.", sessionId);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the SSH session.");
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(sessionId) && client != null && !client.IsConnected)
                {
                    await CloseSessionAsync(sessionId);
                }
            }
        }

        /// <summary>
        /// Ends an active SSH session.
        /// </summary>
        public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            try
            {
                await CloseSessionAsync(sessionId);
                _logger.LogInformation("Session with ID: {SessionId} successfully ended.", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while ending session with ID: {SessionId}.", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Executes a command on an active SSH session.
        /// </summary>
        public async Task<string> ExecuteCommandAsync(string sessionId, string command, CancellationToken cancellationToken)
        {
            SshClient client = null;

            try
            {
                _logger.LogInformation("Executing command '{Command}' for session ID: {SessionId}", command, sessionId);

                if (_sessionManager.ActiveSessions.TryGetValue(sessionId, out client))
                {
                    // Execute the command
                    var cmd = client.CreateCommand(command);
                    var result = cmd.Execute();

                    // Save command details to repository
                    var sshCommand = new SSHCommand
                    {
                        Id = Guid.NewGuid().ToString(),
                        CommandText = command,
                        Output = result,
                        ExecutedAt = DateTime.UtcNow,
                        SSHSessionId = sessionId,
                        ExitCode = cmd.ExitStatus
                    };

                    await _sshCommandRepo.AddAsync(sshCommand);
                    await _sshCommandRepo.SaveChangesAsync();
                    _logger.LogInformation("Command executed and saved for session ID: {SessionId}", sessionId);

                    return result;
                }
                else
                {
                    throw new InvalidOperationException($"Session with ID: {sessionId} not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing command for session ID: {SessionId}.", sessionId);
                throw;
            }
            finally
            {
                if (client != null && !client.IsConnected)
                {
                    await CloseSessionAsync(sessionId);
                }
            }
        }

        /// <summary>
        /// Closes an active SSH session and performs cleanup.
        /// </summary>
        private async Task CloseSessionAsync(string sessionId)
        {
            if (_sessionManager.ActiveSessions.TryRemove(sessionId, out var client))
            {
                try
                {
                    client.Disconnect();
                    client.Dispose();
                    _logger.LogInformation("SSH client for session ID: {SessionId} disconnected.", sessionId);

                    // Update session end time
                    var session = await _sshSessionRepo.GetByIdAsync(sessionId);
                    if (session != null)
                    {
                        session.SessionEndTime = DateTime.UtcNow;
                        await _sshSessionRepo.UpdateAsync(session);
                        await _sshSessionRepo.SaveChangesAsync();
                        _logger.LogInformation("Session with ID: {SessionId} updated in the database.", sessionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while closing session ID: {SessionId}.", sessionId);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Session with ID: {SessionId} not found during cleanup.", sessionId);
            }
        }
    }
}
