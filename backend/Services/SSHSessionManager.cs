// Services/SSHSessionManager.cs
using System.Collections.Concurrent;
using Renci.SshNet;
using Backend.Models.Entities;

namespace Backend.Services
{
    public class SSHSessionManager
    {
        // Manages active SSH sessions with their corresponding SSH clients
        public ConcurrentDictionary<string, SshClient> ActiveSessions { get; } = new ConcurrentDictionary<string, SshClient>();

        // Stores executed commands for each session
        public ConcurrentDictionary<string, List<SSHCommand>> SessionCommands { get; } = new ConcurrentDictionary<string, List<SSHCommand>>();
    }
}
