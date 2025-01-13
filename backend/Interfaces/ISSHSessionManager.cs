using System.Collections.Concurrent;
using Renci.SshNet;
using Backend.Models.Entities.SSH;
namespace Backend.Services
{
    public interface ISSHSessionManager
    {
        ConcurrentDictionary<string, SshClient> ActiveSessions { get; } 
        ConcurrentDictionary<string, List<SSHCommand>> SessionCommands { get; } 

    }
}