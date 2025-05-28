using Microsoft.Extensions.Logging; // Optional: for logging
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json; // Required for GetFromJsonAsync
using System.Text.Json;
using System.Threading.Tasks;
using WiseUpDude.Services.TenorModels;

public class TenorGifService : ITenorGifService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenorGifService> _logger; // Optional logger
    private static readonly Random _random = new Random();
    private const string TenorApiBaseUrl = "https://tenor.googleapis.com/v2/search";

    // HttpClient is injected, ILogger is optional
    public TenorGifService(HttpClient httpClient, ILogger<TenorGifService> logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger; // Can be null if not provided
    }

    /// <summary>
    /// Fetches a URL for a random GIF from Tenor based on a keyword.
    /// </summary>
    /// <param name="apiKey">Your Tenor API key.</param>
    /// <param name="keyword">The search term for the GIF.</param>
    /// <param name="limit">The number of results to fetch from Tenor to pick one randomly. Max 50 for Tenor v2 search.</param>
    /// <returns>A URL string for a GIF, or null if not found or an error occurs.</returns>
    public async Task<string> GetRandomGifUrlAsync(string apiKey, string keyword, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger?.LogError("Tenor API key was null or empty.");
            throw new ArgumentException("Tenor API key cannot be null or empty.", nameof(apiKey));
        }
        if (string.IsNullOrWhiteSpace(keyword))
        {
            _logger?.LogError("Search keyword was null or empty.");
            throw new ArgumentException("Search keyword cannot be null or empty.", nameof(keyword));
        }

        // Tenor API v2 search limit is 50. Adjust if needed.
        if (limit <= 0) limit = 1;
        if (limit > 50) limit = 50;

        // Construct the request URL
        // You can add more parameters like `client_key`, `country`, `locale`, `contentfilter`, `ar_range`
        // `media_filter` helps get specific formats directly, but media_formats in response is more robust.
        var requestUrl = $"{TenorApiBaseUrl}?key={apiKey}&q={Uri.EscapeDataString(keyword)}&limit={limit}&media_filter=minimal";
        // Adding `media_filter=minimal` might make responses smaller if you only care about a few formats.
        // Or specify `media_filter=nanogif,tinygif,gif`

        _logger?.LogInformation("Requesting GIF from Tenor: {RequestUrl}", requestUrl);

        try
        {
            var tenorResponse = await _httpClient.GetFromJsonAsync<TenorApiResponse>(requestUrl);

            if (tenorResponse?.Results != null && tenorResponse.Results.Any())
            {
                var gifsWithUrls = tenorResponse.Results
                    .Where(r => r.MediaFormats?.NanoGif?.Url != null ||
                                r.MediaFormats?.TinyGif?.Url != null ||
                                r.MediaFormats?.MediumGif?.Url != null ||
                                r.MediaFormats?.Gif?.Url != null)
                    .ToArray();

                if (gifsWithUrls.Any())
                {
                    var randomResult = gifsWithUrls[_random.Next(gifsWithUrls.Length)];
                    string selectedUrl = null;

                    // Prioritize smaller GIF formats for "please wait" dialogs
                    if (!string.IsNullOrEmpty(randomResult.MediaFormats.NanoGif?.Url))
                        selectedUrl = randomResult.MediaFormats.NanoGif.Url;
                    else if (!string.IsNullOrEmpty(randomResult.MediaFormats.TinyGif?.Url))
                        selectedUrl = randomResult.MediaFormats.TinyGif.Url;
                    else if (!string.IsNullOrEmpty(randomResult.MediaFormats.MediumGif?.Url)) // MediumGif is often a good balance
                        selectedUrl = randomResult.MediaFormats.MediumGif.Url;
                    else if (!string.IsNullOrEmpty(randomResult.MediaFormats.Gif?.Url)) // Fallback to standard gif
                        selectedUrl = randomResult.MediaFormats.Gif.Url;

                    _logger?.LogInformation("Found GIF for '{Keyword}': {Url}", keyword, selectedUrl);
                    return selectedUrl;
                }
            }

            _logger?.LogWarning("No suitable GIF found for keyword '{Keyword}' from Tenor response.", keyword);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Error fetching GIF from Tenor (HTTP Request). Status Code: {StatusCode}", ex.StatusCode);
            return null;
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Error deserializing Tenor API response.");
            return null;
        }
        catch (Exception ex) // Catch-all for other unexpected errors
        {
            _logger?.LogError(ex, "An unexpected error occurred while fetching GIF from Tenor.");
            return null;
        }
    }
}