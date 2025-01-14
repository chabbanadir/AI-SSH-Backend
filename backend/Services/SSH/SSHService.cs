using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Interfaces;
using Backend.Models.Entities.SSH;
using System.Text.RegularExpressions;
namespace Backend.Services.SSH
{
    public class SSHService : ISSHService
    {
        private readonly ISSHSessionManager _sessionManager;
        private readonly ILogger<SSHService> _logger;
        private readonly IGenericRepository<SSHSession> _sshSessionRepo;
        private readonly IGenericRepository<SSHCommand> _sshCommandRepo;
        private static readonly Regex _ansiRegex = new Regex(@"\x1B\[[0-9;]*[A-Za-z]", RegexOptions.Compiled);

        private string StripAnsiCodes(string text)
        {
            // Return empty or the original text if it's null/empty
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            return _ansiRegex.Replace(text, string.Empty);
        }
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
        /// Start a new SSH session, immediately create a ShellStream, and store it.
        /// All commands for this session will go through the ShellStream from now on.
        /// </summary>
        public async Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken)
        {
            SshClient client = null;
            ShellStream shellStream = null;
            string sessionId = null;

            try
            {
                _logger.LogInformation("Connecting via SSH: {Hostname}:{Port} as {Username}",
                    config.Hostname, config.Port, config.Username);

                // 1) Connect client
                client = new SshClient(config.Hostname, config.Port, config.Username, config.PasswordOrKeyPath);
                client.Connect();

                _logger.LogInformation("SSH connected to {Hostname}:{Port}", config.Hostname, config.Port);

                // 2) Create ShellStream right away
                shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                // 3) Try to get an initial working directory
                shellStream.WriteLine("pwd");
                shellStream.Flush();
                string initialWorkingDirectory = ReadShellStream(shellStream, TimeSpan.FromMilliseconds(200)).Trim();

                _logger.LogInformation("Initial working directory for session: {Dir}", initialWorkingDirectory);

                // 4) Create a new SSHSession record
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

                await _sshSessionRepo.AddAsync(session);
                await _sshSessionRepo.SaveChangesAsync();

                // 5) Add to manager
                if (!_sessionManager.ActiveSessions.TryAdd(sessionId, client))
                {
                    throw new Exception($"Failed to add session {sessionId} to ActiveSessions.");
                }
                if (!_sessionManager.ActiveShellStreams.TryAdd(sessionId, shellStream))
                {
                    throw new Exception($"Failed to add ShellStream for session {sessionId}.");
                }
                _sessionManager.SessionCommands.TryAdd(sessionId, new List<SSHCommand>());

                _logger.LogInformation("SSH session {SessionId} started successfully.", sessionId);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SSH session.");
                throw;
            }
            finally
            {
                // If something fails, ensure cleanup
                if (!string.IsNullOrEmpty(sessionId) && client != null && !client.IsConnected)
                {
                    await CloseSessionAsync(sessionId);
                }
            }
        }

        /// <summary>
        /// Execute a command by writing it to the ShellStream. 
        /// Everything is captured as "Output" (stdout+stderr) in one chunk. 
        /// If you need a real exit code, consider appending `; echo $?` to the command
        /// and parse it from the last line.
        /// </summary>
        public async Task<(string Output, string ErrorOutput, int ExitCode)> ExecuteCommandAsync(
            string sessionId, 
            string command, 
            CancellationToken cancellationToken)
        {
            if (!_sessionManager.ActiveSessions.TryGetValue(sessionId, out var client))
                throw new InvalidOperationException($"No active SSH client found for session {sessionId}.");
            if (!_sessionManager.ActiveShellStreams.TryGetValue(sessionId, out var shellStream))
                throw new InvalidOperationException($"No active ShellStream found for session {sessionId}.");

            try
            {
                _logger.LogInformation("Executing command '{Command}' for session {SessionId}", command, sessionId);

                shellStream.WriteLine(command);
                shellStream.Flush();

                // For a short time, read everything the shell returns
                string rawOutput = ReadShellStream(shellStream, TimeSpan.FromMilliseconds(300));
                string output = StripAnsiCodes(rawOutput);
                // Since ShellStream merges stdout+stderr, we treat it as one. 
                // We'll leave ErrorOutput blank, or you can parse further if needed.
                string errorOutput = string.Empty;
                int exitCode = 0; // default

                // If you want an actual exit code, you can do:
                // shellStream.WriteLine("echo $?");
                // shellStream.Flush();
                // string exitCodeOutput = ReadShellStream(shellStream, TimeSpan.FromMilliseconds(200));
                // parse exitCode from exitCodeOutput...

                // Save command in DB
                var sshCommand = new SSHCommand
                {
                    Id = Guid.NewGuid().ToString(),
                    SSHSessionId = sessionId,
                    CommandText = command,
                    Output = output,
                    ErrorOutput = errorOutput,
                    ExitCode = exitCode,
                    ExecutedAt = DateTime.UtcNow
                };

                await _sshCommandRepo.AddAsync(sshCommand);
                await _sshCommandRepo.SaveChangesAsync();

                return (output, errorOutput, exitCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command '{Command}' for session {SessionId}", command, sessionId);
                throw;
            }
        }

        /// <summary>
        /// End the session: remove from manager, dispose ShellStream, 
        /// disconnect SshClient, mark DB session ended.
        /// </summary>
        public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            try
            {
                await CloseSessionAsync(sessionId);
                _logger.LogInformation("SSH session {SessionId} ended.", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Closes and cleans up everything for a session
        /// </summary>
        private async Task CloseSessionAsync(string sessionId)
        {
            // Remove ShellStream
            if (_sessionManager.ActiveShellStreams.TryRemove(sessionId, out var shellStream))
            {
                shellStream.Close();
                shellStream.Dispose();
            }

            // Remove SshClient
            if (_sessionManager.ActiveSessions.TryRemove(sessionId, out var client))
            {
                client.Disconnect();
                client.Dispose();
            }

            // Update DB: session ended
            var session = await _sshSessionRepo.GetByIdAsync(sessionId);
            if (session != null)
            {
                session.SessionEndTime = DateTime.UtcNow;
                await _sshSessionRepo.UpdateAsync(session);
                await _sshSessionRepo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Read from the ShellStream until there's no more data, or a short timeout.
        /// </summary>
        private string ReadShellStream(ShellStream shellStream, TimeSpan timeout)
        {
            var sb = new StringBuilder();
            var endTime = DateTime.Now.Add(timeout);

            while (DateTime.Now < endTime)
            {
                if (shellStream.DataAvailable)
                {
                    string data = shellStream.Read();
                    if (!string.IsNullOrEmpty(data))
                    {
                        sb.Append(data);
                        // Optionally refresh the timeout if data arrives:
                        endTime = DateTime.Now.Add(timeout);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            return sb.ToString();
        }
    }
}
