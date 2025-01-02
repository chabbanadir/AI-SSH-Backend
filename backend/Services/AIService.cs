// Services/AIService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Interfaces;
namespace Backend.Services{
    public class AIService : IAIService {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:ApiKey"];
        }

        public async Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken)
        {
            var requestContent = new
            {
                prompt = prompt,
                max_tokens = 150,
                temperature = 0.7
            };

            var content = new StringContent(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/engines/davinci/completions", content, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonDocument.Parse(responseString);
            var aiResponse = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();

            return aiResponse?.Trim() ?? string.Empty;
        }
    }
}