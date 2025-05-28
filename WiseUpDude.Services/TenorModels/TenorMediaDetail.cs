using System.Text.Json.Serialization; // Required for JsonPropertyName

namespace WiseUpDude.Services.TenorModels
{
    public class TenorMediaDetail
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("dims")]
        public int[] Dims { get; set; } // Dimensions [width, height]

        [JsonPropertyName("size")]
        public long Size { get; set; } // Size in bytes
    }
}
