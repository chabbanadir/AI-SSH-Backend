// Services/AIService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

using Backend.Interfaces;
using Backend.Models.Entities;
namespace Backend.Services{
    public class AIService : IAIService {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly ILogger<AIService> _logger;

        public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["ApiSettings:Url"] ?? throw new ArgumentNullException("ApiSettings:Url");
            _apiKey = configuration["ApiSettings:Key"] ?? throw new ArgumentNullException("ApiSettings:Key");
            _logger = logger;

        }
        public async Task<string> SendMessageAsync(string message, ConversationContext context)
        {
            _logger.LogInformation("Envoi du message à l'API AI: {Message}", message);

            var requestPayload = new
            {
                contents = context.GetContents(),
                generationConfig = context.GenerationConfig,
                system_instruction = context.SystemInstruction
            };

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var requestJson = JsonConvert.SerializeObject(requestPayload, serializerSettings);
            _logger.LogInformation("Payload envoyé: {Payload}", requestJson);
            
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_apiUrl}?key={_apiKey}", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Payload reçu: {Response}", responseString);

                var responseData = JsonConvert.DeserializeObject<ApiResponse>(responseString) 
                                ?? throw new InvalidOperationException("La réponse de l'API est invalide.");

                // Check if there are any candidates
                if (responseData.Candidates == null || !responseData.Candidates.Any())
                {
                    _logger.LogError("Aucun candidat trouvé dans la réponse de l'IA.");
                    throw new InvalidOperationException("La réponse de l'IA ne contient aucun candidat.");
                }

                var firstCandidate = responseData.Candidates.First();

                // Check if content and parts are present
                if (firstCandidate.Content == null || firstCandidate.Content.Parts == null || !firstCandidate.Content.Parts.Any())
                {
                    _logger.LogError("Le contenu de la réponse de l'IA est incomplet.");
                    throw new InvalidOperationException("Le contenu de la réponse de l'IA est incomplet.");
                }

                var aiText = firstCandidate.Content.Parts.First().Text;

                _logger.LogInformation("Réponse reçue de l'API AI: {ResponseText}", aiText);
                return aiText;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur lors de l'appel à l'API AI.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur inattendue est survenue dans AiService.");
                throw;
            }
        }

    }
    public class ApiResponse{
    [JsonProperty("candidates")]
    public List<Candidate> Candidates { get; set; }

    [JsonProperty("usageMetadata")]
    public UsageMetadata UsageMetadata { get; set; }

    [JsonProperty("modelVersion")]
    public string ModelVersion { get; set; }
    }
    public class Candidate
    {
        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("finishReason")]
        public string FinishReason { get; set; }

        [JsonProperty("avgLogprobs")]
        public double AvgLogprobs { get; set; }
    }

    public class UsageMetadata
    {
        [JsonProperty("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonProperty("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonProperty("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }
}