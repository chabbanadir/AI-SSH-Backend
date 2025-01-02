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

namespace Backend.Services{

    public class SSHService : ISSHService
    {
        
        private readonly ConcurrentDictionary<string, SshClient> _activeSessions = new ConcurrentDictionary<string, SshClient>();
        private readonly AppDbContext _context;
        private readonly IBulkInsertService _bulkInsertService;

        public SSHService(AppDbContext context,IBulkInsertService bulkInsertService)
        {
            _context = context;
            _bulkInsertService = bulkInsertService;

        }

        public async Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var client = new SshClient(config.Hostname, config.Port, config.Username, config.PasswordOrKeyPath);
                client.Connect();

                var session = new SSHSession
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionStartTime = DateTime.UtcNow,
                    UserId = config.UserId,
                    SSHHostConfigId = config.Id,
                    SSHHostConfig = config
                };

                _context.SSHSessions.Add(session);
                _context.SaveChanges();

                _activeSessions.TryAdd(session.Id, client);

                return session;
            }, cancellationToken);
        }

        public async Task ExecuteCommandAsync(string sessionId, string command, CancellationToken cancellationToken)
        {
            if (_activeSessions.TryGetValue(sessionId, out var client))
            {
                await Task.Run(() =>
                {
                    var cmd = client.CreateCommand(command);
                    var result = cmd.Execute();

                    var sshCommand = new SSHCommand
                    {
                        Id = Guid.NewGuid().ToString(),
                        CommandText = command,
                        Output = result,
                        ExecutedAt = DateTime.UtcNow,
                        LinkedSSHSessionId = sessionId
                    };

                    _context.SSHCommands.Add(sshCommand);
                    _context.SaveChanges();
                }, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Session not found or already terminated.");
            }
        }

        public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            if (_activeSessions.TryRemove(sessionId, out var client))
            {
                await Task.Run(async () =>
                {
                    var commands = await _context.SSHCommands
                        .Where(c => c.LinkedSSHSessionId == sessionId)
                        .ToListAsync(cancellationToken);

                    if (commands.Any())
                    {
                        await _bulkInsertService.BulkInsertSSHCommandsAsync(commands, cancellationToken);
                        _context.SSHCommands.RemoveRange(commands);
                    }
                }, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Session not found or already terminated.");
            }
        }
    }
}