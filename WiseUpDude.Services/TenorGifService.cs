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

    // This method is inside your TenorGifService.cs file, within the TenorGifService class

    /// <summary>
    /// Fetches a URL for a random GIF from Tenor based on a keyword.
    /// </summary>
    /// <param name="apiKey">Your Tenor API key.</param>
    /// <param name="keyword">The search term for the GIF.</param>
    /// <param name="limit">The number of results to fetch from Tenor to pick one randomly. Max 50 for Tenor v2 search.</param>
    /// <returns>A URL string for a GIF, or null if not found or an error occurs.</returns>
    // This method is inside your TenorGifService.cs file, within the TenorGifService class

    public async Task<string> GetRandomGifUrlAsync(
        string apiKey,
        string keyword,
        int limit = 20, // Consider increasing the default limit slightly
        string locale = "en_US", // Add locale parameter
        string clientKey = "myWiseUpDudeAppV1") // Add a clientKey parameter (use a unique string for your app)
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

        if (limit <= 0) limit = 1;
        if (limit > 50) limit = 50;

        // --- MODIFIED URL CONSTRUCTION ---
        var requestUrl = $"{TenorApiBaseUrl}?key={apiKey}&q={Uri.EscapeDataString(keyword)}" +
                         $"&client_key={Uri.EscapeDataString(clientKey)}" + // Add client_key
                         $"&locale={Uri.EscapeDataString(locale)}" +         // Add locale
                         $"&limit={limit}" +
                         $"&media_filter=minimal"; // media_filter helps get specific image formats
                                                   // Add any other documented sort/filter parameters here if you find them e.g. &sort=popular

        _logger?.LogInformation("Requesting GIF from Tenor: {RequestUrl}", requestUrl);

        try
        {
            // ... (rest of the method remains the same as the version that prioritizes larger GIF formats) ...
            // The part that fetches, filters for usable formats, selects randomly,
            // and then prioritizes Gif > MediumGif > TinyGif > NanoGif should be kept.

            var tenorResponse = await _httpClient.GetFromJsonAsync<TenorApiResponse>(requestUrl);

            if (tenorResponse?.Results != null && tenorResponse.Results.Any())
            {
                var gifsWithUseableFormats = tenorResponse.Results
                    .Where(r => r.MediaFormats != null &&
                                (!string.IsNullOrEmpty(r.MediaFormats.Gif?.Url) ||
                                 !string.IsNullOrEmpty(r.MediaFormats.MediumGif?.Url) ||
                                 !string.IsNullOrEmpty(r.MediaFormats.TinyGif?.Url) ||
                                 !string.IsNullOrEmpty(r.MediaFormats.NanoGif?.Url)))
                    .ToArray();

                if (gifsWithUseableFormats.Any())
                {
                    var randomResult = gifsWithUseableFormats[_random.Next(gifsWithUseableFormats.Length)];
                    string selectedUrl = null;

                    if (randomResult.MediaFormats != null)
                    {
                        if (!string.IsNullOrEmpty(randomResult.MediaFormats.Gif?.Url))
                        {
                            selectedUrl = randomResult.MediaFormats.Gif.Url;
                            _logger?.LogDebug("Selected 'gif' format for '{Keyword}'. Dimensions: {Dims}", keyword, string.Join("x", randomResult.MediaFormats.Gif.Dims ?? new int[0]));
                        }
                        else if (!string.IsNullOrEmpty(randomResult.MediaFormats.MediumGif?.Url))
                        {
                            selectedUrl = randomResult.MediaFormats.MediumGif.Url;
                            _logger?.LogDebug("Selected 'mediumgif' format for '{Keyword}'. Dimensions: {Dims}", keyword, string.Join("x", randomResult.MediaFormats.MediumGif.Dims ?? new int[0]));
                        }
                        else if (!string.IsNullOrEmpty(randomResult.MediaFormats.TinyGif?.Url))
                        {
                            selectedUrl = randomResult.MediaFormats.TinyGif.Url;
                            _logger?.LogDebug("Selected 'tinygif' format for '{Keyword}'. Dimensions: {Dims}", keyword, string.Join("x", randomResult.MediaFormats.TinyGif.Dims ?? new int[0]));
                        }
                        else if (!string.IsNullOrEmpty(randomResult.MediaFormats.NanoGif?.Url))
                        {
                            selectedUrl = randomResult.MediaFormats.NanoGif.Url;
                            _logger?.LogDebug("Selected 'nanogif' format for '{Keyword}'. Dimensions: {Dims}", keyword, string.Join("x", randomResult.MediaFormats.NanoGif.Dims ?? new int[0]));
                        }
                    }

                    if (!string.IsNullOrEmpty(selectedUrl))
                    {
                        _logger?.LogInformation("Successfully selected GIF URL for '{Keyword}': {Url}", keyword, selectedUrl);
                        return selectedUrl;
                    }
                    else
                    {
                        _logger?.LogWarning("No suitable GIF media format URL found for the selected random result for keyword '{Keyword}'.", keyword);
                    }
                }
                else
                {
                    _logger?.LogWarning("No results with useable media formats found for keyword '{Keyword}' from Tenor response.", keyword);
                }
            }
            else
            {
                _logger?.LogWarning("No results found or results array was empty for keyword '{Keyword}' from Tenor response.", keyword);
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Error fetching GIF from Tenor (HTTP Request). Status Code: {StatusCode}. Keyword: {Keyword}", ex.StatusCode, keyword);
            return null;
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Error deserializing Tenor API response. Keyword: {Keyword}", keyword);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An unexpected error occurred while fetching GIF from Tenor. Keyword: {Keyword}", keyword);
            return null;
        }
    }

 
}