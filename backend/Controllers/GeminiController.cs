// File: Controllers/GeminiController.cs

using Microsoft.AspNetCore.Mvc;
using Backend.Interfaces;
using System.Collections.Concurrent;

namespace Backend.Controllers.Entities
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly IGenerativeAIService _generativeAIService;
        private static readonly ConcurrentDictionary<string, List<string>> _conversationHistory = new();

        public GeminiController(IGenerativeAIService generativeAIService)
        {
            _generativeAIService = generativeAIService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateContent([FromBody] string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BadRequest("Prompt cannot be empty.");
            }

            string userId;
            try
            {
                userId = GetCurrentUserId();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }

            var userHistory = _conversationHistory.GetOrAdd(userId, new List<string>());

            // Apply sliding window (max 10 entries)
            const int MaxHistorySize = 10;
            if (userHistory.Count >= MaxHistorySize)
            {
                userHistory.RemoveAt(0);
            }

            // Construct combined prompt
            string combinedPrompt = string.Join("\n", userHistory) + $"\nUser: {prompt}";

            // Add current prompt to history
            userHistory.Add($"User: {prompt}");

            try
            {
                var generatedText = await _generativeAIService.GenerateTextAsync(combinedPrompt, CancellationToken.None);

                // Add assistant's response to history
                userHistory.Add($"Assistant: {generatedText}");

                return Ok(generatedText);
            }
            catch (HttpRequestException ex)
            {
                // Log the exception (configure a logger if needed)
                return StatusCode(500, $"An error occurred while generating content: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        // Implement GetCurrentUserId() based on your authentication mechanism
        private string GetCurrentUserId()
        {
            // if (User.Identity is not { IsAuthenticated: true })
            // {
            //     throw new UnauthorizedAccessException("User is not authenticated.");
            // }
            return "e36cd350-0621-492a-b350-07689e6c615a";
            // // Retrieve the user ID from claims (adjust the claim type as needed)
            // return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            //        ?? throw new InvalidOperationException("User ID not found in claims.");
        }
    }
}
