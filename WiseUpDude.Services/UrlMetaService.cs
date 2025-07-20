using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WiseUpDude.Shared.Model;

namespace WiseUpDude.Services
{
    public class UrlMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UrlMetaService> _logger;

        public UrlMetaService(HttpClient httpClient, ILogger<UrlMetaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UrlMetaResult> GetUrlMetaAsync(string url)
        {
            string html;
            try
            {
                html = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch URL for meta extraction: {Url}", url);
                return new UrlMetaResult { Title = url, Description = null };
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

            return new UrlMetaResult
            {
                Title = string.IsNullOrWhiteSpace(title) ? url : title,
                Description = description
            };
        }
    }
}
