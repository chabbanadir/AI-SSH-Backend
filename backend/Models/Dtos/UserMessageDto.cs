// Models/DTOs/UserMessageDto.cs
using Newtonsoft.Json;

namespace Backend.Models.Dtos
{
    public class UserMessageDto
    {
        [JsonProperty("userMessage")]
        public string Content { get; set; }
    }
}

