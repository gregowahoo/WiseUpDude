using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WiseUpDude.Shared.Model;

namespace WiseUpDude.Services
{
    public class UrlMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UrlMetaService> _logger;
        private static readonly ConcurrentDictionary<string, UrlMetaResult> _metaCache = new();
        private static readonly ConcurrentDictionary<string, bool> _failedUrls = new();

        public UrlMetaService(HttpClient httpClient, ILogger<UrlMetaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UrlMetaResult> GetUrlMetaAsync(string url)
        {
            if (_metaCache.TryGetValue(url, out var cachedResult))
            {
                return cachedResult;
            }
            if (_failedUrls.ContainsKey(url))
            {
                // Return a default result for failed URLs
                return new UrlMetaResult { Title = url, Description = null };
            }

            string html;
            try
            {
                html = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch URL for meta extraction: {Url}", url);
                _failedUrls[url] = true;
                var errorResult = new UrlMetaResult { Title = url, Description = null };
                _metaCache[url] = errorResult;
                return errorResult;
            }

            string? title = null;
            string? description = null;

            // Extract <title>
            var titleMatch = Regex.Match(html, "<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (titleMatch.Success)
            {
                title = titleMatch.Groups[1].Value.Trim();
            }

            // Extract <meta name=\"description\" content=\"...\">
            var descMatch = Regex.Match(html, "<meta[^>]*name=[\"']description[\"'][^>]*content=[\"']([^\"']*)[\"'][^>]*>", RegexOptions.IgnoreCase);
            if (descMatch.Success)
            {
                description = descMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Try <meta content=\"...\" name=\"description\">
                descMatch = Regex.Match(html, "<meta[^>]*content=[\"']([^\"']*)[\"'][^>]*name=[\"']description[\"'][^>]*>", RegexOptions.IgnoreCase);
                if (descMatch.Success)
                {
                    description = descMatch.Groups[1].Value.Trim();
                }
            }

            var result = new UrlMetaResult
            {
                Title = string.IsNullOrWhiteSpace(title) ? url : title,
                Description = description
            };
            _metaCache[url] = result;
            return result;
        }
    }
}
