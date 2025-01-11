// Models/DTOs/ChatMessage.cs
using Newtonsoft.Json;

namespace Backend.Models.Entities
{
    public class ChatMessage
    {
        [JsonProperty("userMessage")]
        public string UserMessage { get; set; }

        [JsonProperty("aiResponse")]
        public string AiResponse { get; set; }
    }
}
