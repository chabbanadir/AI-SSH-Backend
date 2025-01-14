// Services/ISSHService.cs
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Backend.Models.Entities.SSH;
namespace Backend.Interfaces{
public interface ISSHService
{
    Task<SSHSession> StartSessionAsync(SSHHostConfig config, CancellationToken cancellationToken);
    Task<(string Output, string ErrorOutput, int ExitCode)> ExecuteCommandAsync(string sessionId, string command, CancellationToken cancellationToken);
    Task EndSessionAsync(string sessionId, CancellationToken cancellationToken);
}

}