using System.Text.Json.Serialization; // Required for JsonPropertyName

namespace WiseUpDude.Services.TenorModels
{
    public class TenorMediaFormats
    {
        // These properties will map to keys like "gif", "nanogif" etc. in the JSON
        // System.Text.Json is case-insensitive by default for matching,
        // but explicit JsonPropertyName can be used if needed or if casing differs significantly.
        [JsonPropertyName("gif")]
        public TenorMediaDetail Gif { get; set; }

        [JsonPropertyName("mediumgif")]
        public TenorMediaDetail MediumGif { get; set; }

        [JsonPropertyName("tinygif")]
        public TenorMediaDetail TinyGif { get; set; }

        [JsonPropertyName("nanogif")]
        public TenorMediaDetail NanoGif { get; set; } // Often the best for previews/loading
    }
}
