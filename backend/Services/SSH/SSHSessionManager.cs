using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Renci.SshNet;
using Backend.Models.Entities.SSH;
using Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Services.SSH
{
    public class SSHSessionManager : ISSHSessionManager
    {
        private readonly ILogger<SSHSessionManager> _logger;

        // Internal dictionaries to manage SSH sessions, shell streams, and commands
        private readonly ConcurrentDictionary<string, SshClient> _activeSessions = new ConcurrentDictionary<string, SshClient>();
        private readonly ConcurrentDictionary<string, ShellStream> _activeShellStreams = new ConcurrentDictionary<string, ShellStream>();
        private readonly ConcurrentDictionary<string, List<SSHCommand>> _sessionCommands = new ConcurrentDictionary<string, List<SSHCommand>>();

        public SSHSessionManager(ILogger<SSHSessionManager> logger)
        {
            _logger = logger;
        }

        public bool AddSession(string sessionId, SshClient client, ShellStream shellStream)
        {
            bool addedClient = _activeSessions.TryAdd(sessionId, client);
            bool addedStream = _activeShellStreams.TryAdd(sessionId, shellStream);
            bool addedCommands = _sessionCommands.TryAdd(sessionId, new List<SSHCommand>());

            if (addedClient && addedStream && addedCommands)
            {
                _logger.LogInformation("Session {SessionId} added successfully.", sessionId);
                return true;
            }

            // Cleanup in case of partial addition
            RemoveSession(sessionId);
            _logger.LogWarning("Failed to add session {SessionId}. Partial data cleaned up.", sessionId);
            return false;
        }

        public bool RemoveSession(string sessionId)
        {
            bool removedClient = _activeSessions.TryRemove(sessionId, out var client);
            bool removedStream = _activeShellStreams.TryRemove(sessionId, out var shellStream);
            bool removedCommands = _sessionCommands.TryRemove(sessionId, out var commands);

            if (removedClient)
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
                client.Dispose();
                _logger.LogInformation("SSH client for session {SessionId} disposed.", sessionId);
            }

            if (removedStream)
            {
                shellStream.Close();
                shellStream.Dispose();
                _logger.LogInformation("ShellStream for session {SessionId} disposed.", sessionId);
            }

            if (removedCommands)
            {
                _logger.LogInformation("Commands for session {SessionId} removed.", sessionId);
            }

            return removedClient && removedStream && removedCommands;
        }

        public bool TryGetSessionClient(string sessionId, out SshClient client)
        {
            return _activeSessions.TryGetValue(sessionId, out client);
        }

        public bool TryGetSessionShellStream(string sessionId, out ShellStream shellStream)
        {
            return _activeShellStreams.TryGetValue(sessionId, out shellStream);
        }

        public bool TryGetSessionCommands(string sessionId, out List<SSHCommand> commands)
        {
            return _sessionCommands.TryGetValue(sessionId, out commands);
        }

        public void AddCommand(string sessionId, SSHCommand command)
        {
            if (_sessionCommands.TryGetValue(sessionId, out var commandList))
            {
                lock (commandList)
                {
                    commandList.Add(command);
                }
                _logger.LogInformation("Command added to session {SessionId}.", sessionId);
            }
            else
            {
                _logger.LogWarning("Attempted to add command to non-existent session {SessionId}.", sessionId);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing SSHSessionManager and closing all active SSH sessions.");
            foreach (var sessionId in _activeSessions.Keys.ToList())
            {
                try
                {
                    RemoveSession(sessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing session {SessionId}.", sessionId);
                }
            }
            _logger.LogInformation("All SSH sessions disposed successfully.");
        }
    }
}
