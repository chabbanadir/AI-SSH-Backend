// AiConversationService.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Interfaces.AI;
using Backend.Interfaces;
using Backend.Models.Entities.AI;
using Backend.Models.Entities.SSH;
// Add other necessary namespaces, e.g. for Repositories

namespace Backend.Services.AI
{
    public class AiConversationService : IAiConversationService
    {
        private readonly IAiConversationManager _manager;     // singleton
        private readonly IAIService _aiService;               // can be singleton or httpclient
        private readonly IGenericRepository<SSHSession> _sshSessionRepo;  // scoped
        private readonly IGenericRepository<AIConversation> _conversationRepo; // scoped
        private readonly IGenericRepository<AIMessage> _messageRepo; // scoped

        public AiConversationService(
            IGenericRepository<AIMessage> messageRepo,
            IAiConversationManager manager,
            IAIService aiService,
            IGenericRepository<SSHSession> sshSessionRepo,
            IGenericRepository<AIConversation> conversationRepo
        )
        {
            _messageRepo = messageRepo;
            _manager = manager;
            _aiService = aiService;
            _sshSessionRepo = sshSessionRepo;
            _conversationRepo = conversationRepo;
        }

        public async Task<AIConversation> StartConversationAsync(string sshSessionId)
        {
            // 1) Validate SSH Session from DB
            var sshSession = await _sshSessionRepo.GetByIdAsync(sshSessionId);
            if (sshSession == null)
                throw new InvalidOperationException("SSH session not found.");

            // 2) Create conversation object
            var conversation = new AIConversation
            {
                Id = Guid.NewGuid().ToString(),
                SSHSessionId = sshSession.Id,
                UserId = sshSession.UserId,
                StartedAt = DateTime.UtcNow,
                Topic = $"SSH session conversation - {sshSession.InitialWorkingDirectory ?? "N/A"}"
            };


            // 4) Persist to DB
            await _conversationRepo.AddAsync(conversation);
            await _conversationRepo.SaveChangesAsync();
            
            // 3) Store in memory (singleton dictionary)
            _manager.StartConversation(conversation.Id, conversation);

            return conversation;
        }

        public async Task<AIMessage> AddUserMessageAndGetResponseAsync(string conversationId, string userText)
        {
            // 1) Retrieve conversation from manager
            var conversation = _manager.GetConversation(conversationId);
            if (conversation == null)
            {
                throw new InvalidOperationException("Conversation not found.");
            }

            // 2) Add user message
            var userMessage = new AIMessage
            {
                AIConversationId = conversationId,
                Sender = "user",
                Message = userText,
                SentAt = DateTime.UtcNow
            };
            conversation.AIMessages.Add(userMessage);
            await _messageRepo.AddAsync(userMessage);
            await _messageRepo.SaveChangesAsync();
            await _conversationRepo.SaveChangesAsync();
            // 3) Build context
            var context = BuildConversationContext(conversation);

            // 4) Call external AI
            var aiReplyText = await _aiService.SendMessageAsync(userText, context);

            // 5) Add AI response
            var aiMessage = new AIMessage
            {
                AIConversationId = conversationId,
                Sender = "ai",
                Message = aiReplyText,
                SentAt = DateTime.UtcNow
            };
            await _messageRepo.AddAsync(aiMessage);
            await _messageRepo.SaveChangesAsync();
            conversation.AIMessages.Add(aiMessage);
            await _conversationRepo.SaveChangesAsync();
            return aiMessage;
        }


        public async Task EndConversationAsync(string conversationId)
        {
            // 1) Remove from manager
            var conversation = _manager.RemoveConversation(conversationId);
            if (conversation == null)
            {
                // Maybe it's already ended or wasn't found
                throw new InvalidOperationException("Conversation not found or already removed.");
            }

            // 2) Mark Ended
            conversation.EndedAt = DateTime.UtcNow;

            // 3) Persist to DB
            await _conversationRepo.UpdateAsync(conversation);
            await _conversationRepo.SaveChangesAsync();
        }

        private ConversationContext BuildConversationContext(AIConversation conversation)
        {
            var context = new ConversationContext();
            var initialDir = conversation.Topic?.Replace("SSH session conversation - ", "") ?? "N/A";

            context.SystemInstruction.Parts.Text =
                $"You are a Terminal Unix Administrator named AISSH. " +
                $"Your initial working directory is: {initialDir}. " +
                $"While you can give shell commands when asked for a task, " +
                $"respond with instructions suitable for a Unix terminal. " +
                $"You should also provide direct factual answers when asked. " +
                $"When you provide any answer, structure it in JSON format so it can be used to store multiple commands in order. " +
                $"JSON format: {{\"details\": \"Explanation for the command\", \"Commands\": [\"command1\", \"command2\", ..]}}";

            foreach (var msg in conversation.AIMessages.OrderBy(m => m.SentAt))
            {
                // Convert "user" -> "user"
                //         "ai"   -> "model"
                // (or if you have "system" -> "model" as well, depending on your use case)
                var googleRole = (msg.Sender == "ai") ? "model" 
                            : (msg.Sender == "user") ? "user"
                            : /* default fallback, maybe "user"? */ "user";

                context.Contents.Add(new Content
                {
                    Role = googleRole,
                    Parts = new List<Part>
                    {
                        new Part { Text = msg.Message }
                    }
                });
            }

            return context;
        }
    }
}