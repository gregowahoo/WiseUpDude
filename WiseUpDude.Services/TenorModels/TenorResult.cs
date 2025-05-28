using System.Text.Json.Serialization; // Required for JsonPropertyName

namespace WiseUpDude.Services.TenorModels
{
    public class TenorResult
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("media_formats")]
        public TenorMediaFormats MediaFormats { get; set; } // Maps to "media_formats"

        [JsonPropertyName("content_description")]
        public string ContentDescription { get; set; } // Useful for alt text
    }
}
