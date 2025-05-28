using System.Text.Json.Serialization; // Required for JsonPropertyName
                                      // Models for deserializing Tenor API v2 response
                                      // It's good practice to put these in their own file(s) in a real project (e.g., TenorModels.cs)

namespace WiseUpDude.Services.TenorModels
{
    public class TenorApiResponse
    {
        [JsonPropertyName("results")]
        public TenorResult[] Results { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; } // For pagination, if needed in the future
    }
}
