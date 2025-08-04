using System.Text.Json.Serialization;

namespace WpfAIFileManager.Models
{
    public class SemanticCommandResult
    {
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("extension")]
        public string? Extension { get; set; }

        [JsonPropertyName("createdBefore")]
        public string? CreatedBefore { get; set; }

        [JsonPropertyName("createdAfter")]
        public string? CreatedAfter { get; set; }

        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("source")]
        public List<string>? Source { get; set; }


        [JsonPropertyName("oldName")]
        public string? OldName { get; set; }

        [JsonPropertyName("newName")]
        public string? NewName { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        public string? FileName { get; set; }


    }
}
