using System;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Backend.Interfaces;

namespace Backend.Services
{
    public class AiServiceFactory : IAIServiceFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIService> _logger;

        public AiServiceFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public IAIService CreateAiService()
        {
            var httpClient = _httpClientFactory.CreateClient();
            return new AIService(httpClient, _configuration, _logger);
        }
    }
}
