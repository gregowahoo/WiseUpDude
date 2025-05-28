public interface ITenorGifService
{
    Task<string> GetRandomGifUrlAsync(
        string apiKey,
        string keyword,
        int limit = 20, // Consider increasing the default limit slightly
        string locale = "en_US", // Add locale parameter
        string clientKey = "myWiseUpDudeAppV1");
}
