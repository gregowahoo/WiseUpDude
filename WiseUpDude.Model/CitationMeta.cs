using System.Text.Json.Serialization;

namespace WiseUpDude.Model
{
    public class CitationMeta
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
