using Renci.SshNet;
using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Backend.Interfaces;
using Backend.Models.Entities.SSH;

namespace Backend.Services.SSH
{
    public class SSHService : ISSHService
    {
        private readonly ISSHSessionManager _sessionManager;
        private readonly IGenericRepository<SSHSession> _sshSessionRepo;
        private readonly IGenericRepository<SSHCommand> _sshCommandRepo;
        private readonly ILogger<SSHService> _logger;
        // Fixed unique markers
        private const string CommandStartMarker = "__COMMAND_START__";
        private const string CommandEndMarker = "__COMMAND_END__";
        private const string InitializationMarker = "__INIT_MARKER__";
        private const string ShellPrompt = "SHELL_PROMPT> ";
        // Pre-compiled regex patterns
        private static readonly Regex AnsiRegex = new(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.Compiled);
        private static readonly Regex ExitCodeRegex = new(@"EXIT_CODE:(\d+)", RegexOptions.Compiled);
        private static readonly Regex CommandOutputRegex = new(@"(?<=\b(?:exec env -i /bin/bash --norc --noprofile|export TERM=vt400|export CLICOLOR=0|unalias ls grep)\s*)([\s\S]*?)(?=bash-\d+\.\d+\$)", RegexOptions.Compiled);

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

        public async Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken)
        {
            string sessionId = Guid.NewGuid().ToString();

            try
            {
                // Initialize SSH client with appropriate authentication
                var client = new SshClient(config.Hostname, config.Port, config.Username, config.PasswordOrKeyPath);
                await Task.Run(() => client.Connect(), cancellationToken);

                if (!client.IsConnected)
                {
                    throw new InvalidOperationException("SSH client failed to connect.");
                }

                // Create ShellStream
                var shellStream = client.CreateShellStream("vt400", 80, 24, 800, 600, 1024);

                // Initialize shell with startup commands
                await InitializeShellStreamAsync(shellStream, cancellationToken);

                var session = new SSHSession
                {
                    Id = sessionId,
                    SessionStartTime = DateTime.UtcNow,
                    UserId = config.UserId,
                    SSHHostConfigId = config.Id,
                    SSHHostConfig = config,
                    InitialWorkingDirectory = "~"
                };

                await _sshSessionRepo.AddAsync(session);
                await _sshSessionRepo.SaveChangesAsync();

                // Store client and stream without disposing
                bool added = _sessionManager.AddSession(sessionId, client, shellStream);

                if (!added)
                {
                    throw new InvalidOperationException("Failed to add session to session manager.");
                }

                _logger.LogInformation("SSH session {SessionId} started successfully.", sessionId);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SSH session for config: {ConfigId}", config.Id);

                // Cleanup partially initialized resources
                await CleanupSessionAsync(sessionId);

                throw;
            }
        }

        public async Task<(string Output, string ErrorOutput, int ExitCode)> ExecuteCommandAsync(
        string sessionId,
        string command,
        CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing command for session {SessionId}: {Command}", sessionId, command);

            if (!_sessionManager.TryGetSessionClient(sessionId, out var client))
                throw new InvalidOperationException($"No active SSH client found for session {sessionId}.");

            if (!_sessionManager.TryGetSessionShellStream(sessionId, out var shellStream))
                throw new InvalidOperationException($"No active ShellStream found for session {sessionId}.");

            try
            {
                // Write command to shell with fixed markers and redirect stderr to stdout
                string fullCommand = $"echo {CommandStartMarker}; {command} 2>&1; echo {CommandEndMarker}; echo EXIT_CODE:$?";
                shellStream.WriteLine(fullCommand);
                shellStream.Flush();

                _logger.LogDebug("Command '{Command}' written to ShellStream for session {SessionId}.", fullCommand, sessionId);

                // Read output asynchronously until 'EXIT_CODE:' is found
                var rawOutput = await ReadShellStreamAsync(shellStream, "EXIT_CODE:", TimeSpan.FromSeconds(15), cancellationToken);
                _logger.LogDebug("Raw output received for session {SessionId}: {RawOutput}", sessionId, rawOutput);

                // Extract and clean output
                var commandOutput = ExtractCommandOutput(rawOutput);
                _logger.LogDebug("Cleaned output for session {SessionId}: {CommandOutput}", sessionId, commandOutput);

                var exitCode = ExtractExitCode(rawOutput);
                _logger.LogDebug("Extracted Exit Code for session {SessionId}: {ExitCode}", sessionId, exitCode);

                var sshCommand = new SSHCommand
                {
                    Id = Guid.NewGuid().ToString(),
                    SSHSessionId = sessionId,
                    CommandText = command,
                    Output = commandOutput,
                    ErrorOutput = string.Empty, // Future enhancement: Capture stderr if needed
                    ExitCode = exitCode,
                    ExecutedAt = DateTime.UtcNow
                };

                // Add command to session's list in a thread-safe manner
                _sessionManager.AddCommand(sessionId, sshCommand);

                await _sshCommandRepo.AddAsync(sshCommand);
                await _sshCommandRepo.SaveChangesAsync();

                _logger.LogInformation("Command '{Command}' executed for session {SessionId} with Exit Code {ExitCode}.", command, sessionId, exitCode);

                return (commandOutput, sshCommand.ErrorOutput, exitCode);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Command execution canceled for session {SessionId}.", sessionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing command for session {SessionId}.", sessionId);
                throw;
            }
        }


        public async Task EndSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            await CloseSessionAsync(sessionId, cancellationToken);
        }

        private async Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            try
            {
                bool removed = _sessionManager.RemoveSession(sessionId);

                if (removed)
                {
                    var session = await _sshSessionRepo.GetByIdAsync(sessionId);
                    if (session != null)
                    {
                        session.SessionEndTime = DateTime.UtcNow;
                        await _sshSessionRepo.UpdateAsync(session);
                        await _sshSessionRepo.SaveChangesAsync();
                        _logger.LogInformation("SSH session {SessionId} ended.", sessionId);
                    }
                }
                else
                {
                    _logger.LogWarning("Attempted to end non-existent session {SessionId}.", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing SSH session {SessionId}.", sessionId);
                throw;
            }
        }

        private async Task CleanupSessionAsync(string sessionId)
        {
            // Delegate cleanup to SSHSessionManager
            _sessionManager.RemoveSession(sessionId);

            // Optionally, remove commands list if needed
            // Already handled in RemoveSession

            await Task.CompletedTask;
        }

       private async Task InitializeShellStreamAsync(ShellStream shellStream, CancellationToken cancellationToken)
        {
            var initCommands = new[]
            {
                "exec env -i /bin/bash --norc --noprofile", // Start a clean shell environment
                $"export TERM=vt400",
                $"export CLICOLOR=0",
                $"unalias ls grep",
                $"export PS1='{ShellPrompt}'", // Set a unique, simple prompt
                $"echo {InitializationMarker}__INIT_DONE__{InitializationMarker}"
            };

            foreach (var cmd in initCommands)
            {
                shellStream.WriteLine(cmd);
                shellStream.Flush();
                _logger.LogDebug("Initialization command '{Command}' sent.", cmd);
                await Task.Delay(100, cancellationToken); // Increased delay to allow command execution
            }

            // Wait for the initialization completion marker
            var output = await ReadShellStreamAsync(shellStream, $"{InitializationMarker}__INIT_DONE__{InitializationMarker}", TimeSpan.FromSeconds(10), cancellationToken);
            _logger.LogDebug("Shell initialization completed with output: {Output}", output);
        }

        private async Task<string> ReadShellStreamAsync(ShellStream shellStream, string marker, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var buffer = new StringBuilder();
            var endTime = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < endTime)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (shellStream.DataAvailable)
                {
                    string chunk = shellStream.Read();
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        buffer.Append(chunk);
                        _logger.LogDebug("Read from shellStream: {Chunk}", chunk);

                        // Check for the specific marker to signal completion
                        if (chunk.Contains(marker))
                        {
                            break;
                        }

                        // Reset the timeout when data is received
                        endTime = DateTime.UtcNow + timeout;
                    }
                }
                else
                {
                    await Task.Delay(50, cancellationToken);
                }
            }

            string finalOutput = buffer.ToString();

            // Verify that we detected the marker
            if (!finalOutput.Contains(marker))
            {
                _logger.LogWarning("Marker '{Marker}' not found in shell output.", marker);
                throw new TaskCanceledException($"Timeout waiting for marker '{marker}'.");
            }

            return finalOutput;
        }


        private string ExtractCommandOutput(string rawOutput)
        {
            // Remove ANSI codes and control characters
            var cleanedOutput = StripAnsiCodes(rawOutput);
            //cleanedOutput = RemoveControlCharacters(cleanedOutput);

            // Find the first occurrence of __COMMAND_START__
            int firstStart = cleanedOutput.IndexOf(CommandStartMarker, StringComparison.Ordinal);
            if (firstStart < 0)
            {
                _logger.LogWarning("First start marker '{Marker}' not found in output.", CommandStartMarker);
                return string.Empty;
            }

            // Find the first occurrence of __COMMAND_END__ after the first __COMMAND_START__
            int firstEnd = cleanedOutput.IndexOf(CommandEndMarker, firstStart + CommandStartMarker.Length, StringComparison.Ordinal);
            if (firstEnd < 0)
            {
                _logger.LogWarning("First end marker '{Marker}' not found in output.", CommandEndMarker);
                return string.Empty;
            }

            // Find the second occurrence of __COMMAND_START__ after the first __COMMAND_END__
            int secondStart = cleanedOutput.IndexOf(CommandStartMarker, firstEnd + CommandEndMarker.Length, StringComparison.Ordinal);
            if (secondStart < 0)
            {
                _logger.LogWarning("Second start marker '{Marker}' not found in output.", CommandStartMarker);
                return string.Empty;
            }

            // Find the second occurrence of __COMMAND_END__ after the second __COMMAND_START__
            int secondEnd = cleanedOutput.IndexOf(CommandEndMarker, secondStart + CommandStartMarker.Length, StringComparison.Ordinal);
            if (secondEnd < 0)
            {
                _logger.LogWarning("Second end marker '{Marker}' not found in output.", CommandEndMarker);
                return string.Empty;
            }

            // Extract the command output between the second markers
            string commandOutput = cleanedOutput.Substring(
                secondStart + CommandStartMarker.Length,
                secondEnd - (secondStart + CommandStartMarker.Length)
            ).Trim();

            return commandOutput;
        }



        private int ExtractExitCode(string rawOutput)
        {
            var match = ExitCodeRegex.Match(rawOutput);
            return match.Success && int.TryParse(match.Groups[1].Value, out int exitCode) ? exitCode : -1;
        }

        private string StripAnsiCodes(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : AnsiRegex.Replace(text, string.Empty);
        }

        // private string RemoveControlCharacters(string text)
        // {
        //     return string.IsNullOrWhiteSpace(text) ? string.Empty : Regex.Replace(text, @"[\x00-\x1F\x7F]", string.Empty);
        // }
    }
}
