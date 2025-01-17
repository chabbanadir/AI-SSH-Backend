using System.Collections.Concurrent;
using Renci.SshNet;
using Backend.Models.Entities.SSH;

namespace Backend.Interfaces
{
    public interface ISSHSessionManager : IDisposable
    {
        bool AddSession(string sessionId, SshClient client, ShellStream shellStream);
        bool RemoveSession(string sessionId);
        bool TryGetSessionClient(string sessionId, out SshClient client);
        bool TryGetSessionShellStream(string sessionId, out ShellStream shellStream);
        bool TryGetSessionCommands(string sessionId, out List<SSHCommand> commands);
        void AddCommand(string sessionId, SSHCommand command);
    }
}
