// Services/ISSHService.cs
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Backend.Models.Entities;
namespace Backend.Interfaces{
    public interface ISSHService
    {
        Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken);
        Task ExecuteCommandAsync(string sessionId, string command, CancellationToken cancellationToken);
        Task EndSessionAsync(string sessionId, CancellationToken cancellationToken);
    }
}