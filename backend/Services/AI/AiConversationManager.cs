// AiConversationManager.cs

using System;
using System.Collections.Concurrent;
using Backend.Interfaces.AI;
using Backend.Models.Entities.AI;
using Backend.Interfaces; // If you're using a generic repo
// using your EF repositories or Data layer

namespace Backend.Services.AI
{
    public class AiConversationManager : IAiConversationManager
    {
        // Thread-safe dictionary
        private readonly ConcurrentDictionary<string, AIConversation> _conversations
            = new ConcurrentDictionary<string, AIConversation>();

        public AIConversation StartConversation(string conversationId, AIConversation conversation)
        {
            // Store in dictionary
            _conversations[conversationId] = conversation;
            return conversation;
        }

        public AIConversation? GetConversation(string conversationId)
        {
            _conversations.TryGetValue(conversationId, out var convo);
            return convo;
        }

        public AIConversation? RemoveConversation(string conversationId)
        {
            if (_conversations.TryRemove(conversationId, out var convo))
            {
                return convo;
            }
            return null;
        }
    }
}
