// Services/IAIService.cs
using System.Threading;
using System.Threading.Tasks;
using Backend.Models.Entities;
namespace Backend.Interfaces{
    public interface IAIService
    {
        Task<string> SendMessageAsync(string message, ConversationContext context);
    }
}