public interface ITenorGifService
{
    Task<string> GetRandomGifUrlAsync(string apiKey, string keyword, int limit = 10);
}
