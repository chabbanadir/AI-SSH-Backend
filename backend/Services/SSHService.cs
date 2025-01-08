using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Backend.Interfaces;
using Backend.Models.Entities;
using Backend.Context;
using Backend.Services;
using Microsoft.Extensions.Logging;

namespace Backend.Services{

    public class SSHService : ISSHService
    {
        // Manage Sessions and commands 
        private readonly ISSHSessionManager _sessionManager;
        // Manages active SSH sessions with their corresponding SSH clients
        private readonly ILogger<SSHService> _logger;
        // Stores executed commands for each session
        private readonly IGenericRepository<SSHSession> _sshSessionRepo;
        private readonly IGenericRepository<SSHCommand> _sshCommandRepo;
        private readonly IBulkInsertService _bulkInsertService;

        public SSHService(
            ISSHSessionManager sessionManager,
            IGenericRepository<SSHSession> sshSessionRepo,
            IGenericRepository<SSHCommand> sshCommandRepo,
            IBulkInsertService bulkInsertService,
            ILogger<SSHService> logger)
        {
            _sessionManager = sessionManager;
            _sshSessionRepo = sshSessionRepo;
            _sshCommandRepo = sshCommandRepo;
            _bulkInsertService = bulkInsertService;
            _logger = logger;
        }
        public async Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Trying to connect SSH to {Hostname}:{Port} as {Username}", config.Hostname, config.Port, config.Username);
            var client = new SshClient(config.Hostname, config.Port, config.Username, config.PasswordOrKeyPath);
            client.Connect();
            _logger.LogInformation("SSH connected to {Hostname}:{Port}", config.Hostname, config.Port);

            var session = new SSHSession
            {
                Id = Guid.NewGuid().ToString(),
                SessionStartTime = DateTime.UtcNow,
                UserId = config.UserId,
                SSHHostConfigId = config.Id,
                SSHHostConfig = config
            };
            _logger.LogInformation("Saving the session with ID: {SessionId} to the repository", session.Id);
            await _sshSessionRepo.AddAsync(session);
            await _sshSessionRepo.SaveChangesAsync(); // Ensure changes are saved

            _logger.LogInformation("Adding session with ID: {SessionId} to active sessions", session.Id);
            bool added = _sessionManager.ActiveSessions.TryAdd(session.Id, client);
            if (!added)
            {
                _logger.LogError("Failed to add session with ID: {SessionId} to active sessions.", session.Id);
                throw new Exception("Failed to add SSH session to active sessions.");
            }
            _logger.LogInformation("Session with ID: {SessionId} added successfully.", session.Id);

            // Initialize the command list for this session
            _sessionManager.SessionCommands.TryAdd(session.Id, new List<SSHCommand>());

            return session;
        }   

        public async Task<string> ExecuteCommandAsync(string sessionId, string command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing command '{Command}' for session ID: {SessionId}", command, sessionId);
            if (_sessionManager.ActiveSessions.TryGetValue(sessionId, out var client))
            {
                _logger.LogInformation("Session found. Executing command: {Command}", command);

                var cmd = client.CreateCommand(command);
                var result = cmd.Execute();

                var sshCommand = new SSHCommand
                {
                    Id = Guid.NewGuid().ToString(),
                    CommandText = command,
                    Output = result,
                    ExecutedAt = DateTime.UtcNow,
                    SSHSessionId = sessionId,
                    ExitCode = cmd.ExitStatus // Correctly assigning the exit status
                };

                // Add the command to the in-memory list for bulk insertion later
                if (_sessionManager.SessionCommands.TryGetValue(sessionId, out var commandList))
                {
                    lock (commandList) // Ensure thread safety
                    {
                        commandList.Add(sshCommand);
                    }
                }
                else
                {
                    _logger.LogError("Session command list not found for session ID: {SessionId}", sessionId);
                    throw new InvalidOperationException("Session command list not found.");
                }

                // Await the asynchronous repository methods
                await _sshCommandRepo.AddAsync(sshCommand);
                await _sshCommandRepo.SaveChangesAsync();

                _logger.LogInformation("Command '{Command}' executed successfully for session ID: {SessionId}", command, sessionId);
                return result; // Return the command output
            }
            else
            {
                _logger.LogError("Session with ID: {SessionId} not found or already terminated.", sessionId);
                throw new InvalidOperationException("Session not found or already terminated.");
            }
        }


        public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
             if (_sessionManager.ActiveSessions.TryRemove(sessionId, out var client))
            {
                client.Disconnect();
                client.Dispose();

                // Retrieve and bulk insert SSHCommands related to this session
                if (_sessionManager.SessionCommands.TryRemove(sessionId, out var commands))
                {
                    if (commands.Any())
                    {
                        await _bulkInsertService.BulkInsertSSHCommandsAsync(commands, cancellationToken);
                        // Optionally, clear the list if needed (already removed from _sessionCommands)
                    }
                }

                // Optionally, handle other related entities like AIConversations and AIMessages here
            }
            else
            {
                _logger.LogError("Session with ID: {SessionId} not found or already terminated.", sessionId);
                throw new InvalidOperationException("Session not found or already terminated.");
            }
        }
    }
}