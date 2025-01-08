// File: Services/GenerativeAI/GenerativeAIService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Backend.Interfaces;
using Backend.Models.Entities;

namespace Backend.Services
{
    public class GenerativeAIService : IGenerativeAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GenerativeAIService> _logger;
        private readonly string _apiKey;
        private readonly string _modelName;

        public GenerativeAIService(HttpClient httpClient, ILogger<GenerativeAIService> logger, IOptions<GeminiOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = options.Value.ApiKey;
            _modelName = options.Value.ModelName; // Add ModelName to GeminiOptions
        }

        public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken)
        {
            var requestUri = $"https://generativelanguage.googleapis.com/v1/models/{_modelName}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug($"Sending request to Gemini API: {requestUri}");
            _logger.LogDebug($"Request Payload: {jsonContent}");

            var response = await _httpClient.PostAsync(requestUri, httpContent, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gemini API Error: {response.StatusCode}, Body: {responseBody}");
                throw new HttpRequestException($"Gemini API responded with status code {response.StatusCode}: {responseBody}");
            }

            _logger.LogDebug($"Gemini API Response: {responseBody}");

            // Deserialize the response to extract the generated text
            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);

            if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
            {
                _logger.LogError("No candidates returned in Gemini API response.");
                throw new InvalidOperationException("Gemini API did not return any candidates.");
            }

            var generatedText = geminiResponse.Candidates[0].Content?.Parts?[0]?.Text;

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                _logger.LogError("Generated text is empty.");
                throw new InvalidOperationException("Gemini API returned empty text.");
            }

            return generatedText;
        }
    }
}
