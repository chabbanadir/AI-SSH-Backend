// Services/IBulkInsertService.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Models.Entities;
namespace Backend.Interfaces{
    public interface IBulkInsertService
    {
        Task BulkInsertSSHCommandsAsync(IEnumerable<SSHCommand> commands, CancellationToken cancellationToken);
        Task BulkInsertAIConversationsAsync(IEnumerable<AIConversation> conversations, CancellationToken cancellationToken);
        Task BulkInsertAIMessagesAsync(IEnumerable<AIMessage> messages, CancellationToken cancellationToken);
    }
}