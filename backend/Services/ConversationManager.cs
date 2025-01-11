using System.Collections.Generic;
using Backend.Models.Entities;
using Backend.Interfaces;

namespace Backend.Services{
public class ConversationManager
    {
        private readonly ConversationContext _context;
        private readonly IGenericRepository<AIConversation> _conversationRepository;
        private AIConversation _currentConversation;

        public ConversationManager(IGenericRepository<AIConversation> conversationRepository)
        {
            _conversationRepository = conversationRepository;
            // Initialize or retrieve the current conversation
            _currentConversation = new AIConversation
            {
                StartedAt = DateTime.UtcNow,
                // Initialize other necessary properties
            };

            _context = new ConversationContext();
        }
        public async Task InitializeConversationAsync(string userId)
        {
            _currentConversation = new AIConversation
            {
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                // Initialize other necessary properties
            };
            await _conversationRepository.AddAsync(_currentConversation);
            await _conversationRepository.SaveChangesAsync();
        }

        public async Task AddUserMessageAsync(string message)
        {
            var aiMessage = new AIMessage
            {
                Sender = "user",
                Message = message,
                SentAt = DateTime.UtcNow,
                AIConversationId = _currentConversation.Id
            };
            _currentConversation.AIMessages.Add(aiMessage);
            await _conversationRepository.SaveChangesAsync();
        }

         public async Task AddAiResponseAsync(string response)
        {
            var aiMessage = new AIMessage
            {
                Sender = "ai",
                Message = response,
                SentAt = DateTime.UtcNow,
                AIConversationId = _currentConversation.Id
            };
            _currentConversation.AIMessages.Add(aiMessage);
            await _conversationRepository.SaveChangesAsync();
        }

        public AIConversation GetCurrentConversation()
        {
            return _currentConversation;
        }

        public async Task EndConversationAsync()
        {
            _currentConversation.EndedAt = DateTime.UtcNow;
            await _conversationRepository.SaveChangesAsync();
        }
        public ConversationContext GetContext()
        {
            return _context;
        }
    }
}