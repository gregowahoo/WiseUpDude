using System.Text.Json.Serialization;

namespace WiseUpDude.Model
{
    public class TopicItem
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
