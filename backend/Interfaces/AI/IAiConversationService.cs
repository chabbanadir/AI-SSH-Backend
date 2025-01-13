// IAiConversationService.cs

using System.Threading.Tasks;
using Backend.Models.Entities.AI;

namespace Backend.Interfaces.AI
{
    public interface IAiConversationService
    {
        Task<AIConversation> StartConversationAsync(string sshSessionId);
        Task<AIMessage> AddUserMessageAndGetResponseAsync(string conversationId, string userText);
        Task EndConversationAsync(string conversationId);
    }
}
