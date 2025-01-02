// Controllers/AIConversationController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Interfaces;
using Backend.Context;
using Backend.Models.Entities;

namespace Backend.Controllers.Entities{

    [ApiController]
    [Route("api/[controller]")]
    public class AIConversationController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly AppDbContext _context;
        private readonly IBulkInsertService _bulkInsertService;

        public AIConversationController(IAIService aiService, AppDbContext context, IBulkInsertService bulkInsertService)
        {
            _aiService = aiService;
            _context = context;
            _bulkInsertService = bulkInsertService;
        }

        // POST: api/AIConversation/{sessionId}/Start
        [HttpPost("{sessionId}/Start")]
        public async Task<ActionResult<AIConversation>> StartConversation(string sessionId, StartConversationRequest request, CancellationToken cancellationToken)
        {
            var session = await _context.SSHSessions.FindAsync(new object[] { sessionId }, cancellationToken);
            if (session == null)
            {
                return NotFound("SSH Session not found.");
            }

            var conversation = new AIConversation
            {
                Id = Guid.NewGuid().ToString(),
                Topic = request.Topic,
                LinkedSSHSessionId = sessionId
            };

            _context.AIConversations.Add(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }

        // GET: api/AIConversation/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AIConversation>> GetConversation(string id, CancellationToken cancellationToken)
        {
            var conversation = await _context.AIConversations
                .Include(c => c.AIMessages)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (conversation == null)
            {
                return NotFound();
            }

            return conversation;
        }

        // POST: api/AIConversation/{id}/SendMessage
        [HttpPost("{id}/SendMessage")]
        public async Task<ActionResult<AIMessage>> SendMessage(string id, SendMessageRequest request, CancellationToken cancellationToken)
        {
            var conversation = await _context.AIConversations.FindAsync(new object[] { id }, cancellationToken);
            if (conversation == null)
            {
                return NotFound("AI Conversation not found.");
            }

            var aiMessage = new AIMessage
            {
                Id = Guid.NewGuid().ToString(),
                Message = request.Message,
                SentAt = DateTime.UtcNow,
                AIConversationId = id
            };

            // Add the user message
            _context.AIMessages.Add(aiMessage);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate AI response
            var aiResponse = await _aiService.GenerateContentAsync(request.Message, cancellationToken);

            var responseMessage = new AIMessage
            {
                Id = Guid.NewGuid().ToString(),
                Message = aiResponse,
                SentAt = DateTime.UtcNow,
                AIConversationId = id
            };

            _context.AIMessages.Add(responseMessage);
            await _context.SaveChangesAsync(cancellationToken);

            // Optionally, you can queue bulk insert here or handle it on session end

            return Ok(responseMessage);
        }

          // POST: api/AIConversation/{id}/End
    [HttpPost("{id}/End")]
    public async Task<IActionResult> EndConversation(string id, CancellationToken cancellationToken)
    {
        var conversation = await _context.AIConversations.FindAsync(new object[] { id }, cancellationToken);
        if (conversation == null)
        {
            return NotFound("AI Conversation not found.");
        }

        try
        {
            // Retrieve AIConversations and AIMessages related to this conversation
            var conversations = await _context.AIConversations.Where(c => c.Id == id).ToListAsync(cancellationToken);
            var messages = await _context.AIMessages.Where(m => m.AIConversationId == id).ToListAsync(cancellationToken);

            if (conversations.Any())
            {
                await _bulkInsertService.BulkInsertAIConversationsAsync(conversations, cancellationToken);
                _context.AIConversations.RemoveRange(conversations);
            }

            if (messages.Any())
            {
                await _bulkInsertService.BulkInsertAIMessagesAsync(messages, cancellationToken);
                _context.AIMessages.RemoveRange(messages);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Conversation ended and data bulk inserted successfully." });
        }
        catch (Exception ex)
        {
            // Log the exception (implement logging as needed)
            return StatusCode(500, "An error occurred while ending the conversation.");
        }
    }

    // DTOs for requests
    public class StartConversationRequest
    {
        public string Topic { get; set; }
    }

    public class SendMessageRequest
    {
        public string Message { get; set; }
    }
    }
}