using System.Text.Json.Serialization;

namespace WiseUpDude.Model
{
    public class Topic
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } // Added Id property

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

    }
}
