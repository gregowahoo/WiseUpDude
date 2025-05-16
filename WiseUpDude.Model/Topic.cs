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

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty; // Added Category property

        [JsonPropertyName("categoryDescription")]
        public string CategoryDescription { get; set; } = string.Empty; // Added CategoryDescription property
        public int CategoryId { get; set; }
    }
}
