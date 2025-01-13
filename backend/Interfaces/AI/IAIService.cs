// Services/IAIService.cs
using System.Threading;
using System.Threading.Tasks;
using Backend.Models.Entities.AI;
namespace Backend.Interfaces.AI{
    public interface IAIService
    {
        Task<string> SendMessageAsync(string message, ConversationContext context);
    }
}