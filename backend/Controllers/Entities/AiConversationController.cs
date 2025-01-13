// AiConversationController.cs

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Interfaces.AI;
using Backend.Models.Entities.AI;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers.Entities
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiConversationController : ControllerBase
    {
        private readonly IAiConversationService _service;

        public AiConversationController(IAiConversationService service)
        {
            _service = service;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartConversation([FromQuery] string sshSessionId)
        {
            //var userId = User?.Identity?.Name ?? "TestUser";
            var convo = await _service.StartConversationAsync(
                sshSessionId 
                //userId
                );
            return Ok(new { conversationId = convo.Id });
        }

        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> SendUserMessage(string conversationId, [FromBody] MessageRequest request)
        {
            var aiMessage = await _service.AddUserMessageAndGetResponseAsync(conversationId, request.Message);
            return Ok(new { aiMessage = aiMessage.Message });
        }

        [HttpPost("{conversationId}/end")]
        public async Task<IActionResult> EndConversation(string conversationId)
        {
            await _service.EndConversationAsync(conversationId);
            return Ok("Conversation ended.");
        }
    }

    public class MessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
