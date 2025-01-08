// File: Models/Gemini/GeminiOptions.cs
namespace Backend.Models.Entities
{
    public sealed class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "gemini-1.5-flash"; // Default or set via configuration
    }
}
