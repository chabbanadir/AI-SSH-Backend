// Services/SSHSessionManager.cs
using System.Collections.Concurrent;
using Renci.SshNet;
using Backend.Models.Entities.SSH;
using Backend.Interfaces;
namespace Backend.Services.SSH
{
    public class SSHSessionManager :ISSHSessionManager
    {
        // Manages active SSH sessions with their corresponding SSH clients
        public ConcurrentDictionary<string, SshClient> ActiveSessions { get; } = new ConcurrentDictionary<string, SshClient>();

        // Stores executed commands for each session
        public ConcurrentDictionary<string, List<SSHCommand>> SessionCommands { get; } = new ConcurrentDictionary<string, List<SSHCommand>>();
    }
}
