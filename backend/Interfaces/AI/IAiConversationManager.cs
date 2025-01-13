// IAiConversationManager.cs

using Backend.Models.Entities.AI;

namespace Backend.Interfaces.AI
{
    public interface IAiConversationManager
    {
        AIConversation StartConversation(string conversationId, AIConversation conversation);
        AIConversation? GetConversation(string conversationId);
        AIConversation? RemoveConversation(string conversationId);
    }
}
