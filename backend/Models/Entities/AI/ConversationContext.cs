using System.Collections.Generic;
using Newtonsoft.Json;

namespace Backend.Models.Entities.AI{
public class ConversationContext
    {
        [JsonProperty("contents")]
        public List<Content> Contents { get; set; } = new List<Content>();

        [JsonProperty("generationConfig")]
        public GenerationConfig GenerationConfig { get; set; } = new GenerationConfig();

        [JsonProperty("system_instruction")]
        public SystemInstruction SystemInstruction { get; set; } = new SystemInstruction();

        public ConversationContext()
        {
            // Initialisation des contenus
           

            // Initialisation de la configuration de génération
            GenerationConfig = new GenerationConfig
            {
                StopSequences = new List<string> { "Title" },
                Temperature = 1.0,
                MaxOutputTokens = 800,
                TopP = 0.8,
                TopK = 10
            };

            // Initialisation des instructions système
            SystemInstruction = new SystemInstruction
            {
                Parts = new Part { Text = "You are a Terminal Unix Administrator. Your name is AISSH." }
            };
        }
         public object GetContents()
        {
            return Contents;
        }
    }

    public class Content
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("parts")]
        public List<Part> Parts { get; set; } = new List<Part>();
    }

    public class Part
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GenerationConfig
    {
        [JsonProperty("stopSequences")]
        public List<string> StopSequences { get; set; } = new List<string>();

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }

        [JsonProperty("topP")]
        public double TopP { get; set; }

        [JsonProperty("topK")]
        public int TopK { get; set; }
    }

    public class SystemInstruction
    {
        [JsonProperty("parts")]
        public Part Parts { get; set; } = new Part();
    }
}