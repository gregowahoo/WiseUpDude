using System.Text.Json.Serialization;

namespace WiseUpDude.Model
{
    public class Topic
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("llm")]
        public string Llm{ get; set; } = string.Empty;
    }
}
