// Models/DTOs/AiResponseDto.cs
using Newtonsoft.Json;

namespace Backend.Models.Dtos
{
    public class AiResponseDto
    {
        [JsonProperty("aiResponse")]
        public string Content { get; set; }
    }
}
