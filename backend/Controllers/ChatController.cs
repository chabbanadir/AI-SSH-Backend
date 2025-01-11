using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Backend.Interfaces;
using Backend.Models.Dtos;
using Backend.Services;
using Microsoft.Extensions.Logging;
using Backend.Models.Entities; // Assurez-vous que ce using est correct
using Newtonsoft.Json;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAIServiceFactory _aiServiceFactory;
        private readonly ConversationManager _conversationManager;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IAIServiceFactory aiServiceFactory, ConversationManager conversationManager, ILogger<ChatController> logger)
        {
            _aiServiceFactory = aiServiceFactory;
            _conversationManager = conversationManager;
            _logger = logger;
        }
        [HttpPost("start")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest("User ID is required to start a conversation.");
            }

            try
            {
                await _conversationManager.InitializeConversationAsync(request.UserId);
                return Ok(new { ConversationId = _conversationManager.GetCurrentConversation().Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start a conversation.");
                return StatusCode(500, "Failed to start a conversation.");
            }
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserMessage))
            {
                return BadRequest("Le message utilisateur ne peut pas être vide.");
            }

            try
            {
                // Add user message to conversation
                await _conversationManager.AddUserMessageAsync(request.UserMessage);

                // Retrieve the current conversation context
                var conversation = _conversationManager.GetCurrentConversation();
                var context = new ConversationContext
                {
                    Contents = conversation.AIMessages.Select(msg => new Content
                    {
                        Role = msg.Sender,
                        Parts = new List<Part> { new Part { Text = msg.Message } }
                    }).ToList(),
                    GenerationConfig = new GenerationConfig
                    {
                        StopSequences = new List<string> { "Title" }, // Example stop sequence
                        Temperature = 2, // Adjusted to a typical value
                        MaxOutputTokens = 500, // Set to a reasonable number
                        TopP = 0.9, // Adjusted to a typical value
                        TopK = 40 // Adjusted to a typical value
                    },
                    SystemInstruction = new SystemInstruction
                    {
                        Parts = new Part { Text = "You are a Terminal Unix Administrator. Your name is AISSH." }
                    }
                };

                // Create an instance of IAIService via the factory
                var aiService = _aiServiceFactory.CreateAiService();

                // Send the message to the AI
                var aiResponse = await aiService.SendMessageAsync(request.UserMessage, context);

                // Add the AI response to conversation
                await _conversationManager.AddAiResponseAsync(aiResponse);

                // Return the AI response
                return Ok(new AiResponseDto { Content = aiResponse });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur lors de l'appel à l'API AI.");
                return StatusCode(503, "Service indisponible. Veuillez réessayer plus tard.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur inattendue est survenue.");
                return StatusCode(500, "Une erreur inattendue est survenue.");
            }
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndConversation()
        {
            try
            {
                await _conversationManager.EndConversationAsync();
                return Ok("Conversation ended successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to end the conversation.");
                return StatusCode(500, "Failed to end the conversation.");
            }
        }
    }

      
    

}
  // [HttpPost("message")]
        // public async Task<IActionResult> SendMessage([FromBody] ChatMessage request)
        // {
        //     if (request == null || string.IsNullOrWhiteSpace(request.UserMessage))
        //     {
        //         return BadRequest("Le message utilisateur ne peut pas être vide.");
        //     }

        //     try
        //     {
        //         // Ajouter le message utilisateur à l'historique
        //         _conversationManager.AddUserMessage(request.UserMessage);

        //         // Récupérer le contexte actuel
        //         var context = _conversationManager.GetContext();

        //         // Créer une instance de IAIService via la factory
        //         var aiService = _aiServiceFactory.CreateAiService();

        //         // Envoyer le message à l'IA
        //         var aiResponse = await aiService.SendMessageAsync(request.UserMessage, context);

        //         // Ajouter la réponse de l'IA à l'historique
        //         _conversationManager.AddAiResponse(aiResponse);

        //         return Ok(aiResponse);
        //     }
        //     catch (HttpRequestException ex)
        //     {
        //         _logger.LogError(ex, "Erreur lors de l'appel à l'API AI.");
        //         return StatusCode(503, "Service indisponible. Veuillez réessayer plus tard.");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Une erreur inattendue est survenue.");
        //         return StatusCode(500, "Une erreur inattendue est survenue.");
        //     }
        // }