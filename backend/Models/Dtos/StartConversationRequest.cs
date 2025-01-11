 // Models/DTOs/StartConversationRequest.cs
    using Newtonsoft.Json;

    namespace Backend.Models.Dtos
    {
        public class StartConversationRequest
        {
            [JsonProperty("userId")]
            public string UserId { get; set; }
        }
    }