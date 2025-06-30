using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WiseUpDude.Services
{
    public class UrlMetaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UrlMetaService> _logger;

        public UrlMetaService(IHttpClientFactory httpClientFactory, ILogger<UrlMetaService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public class UrlMetaResult
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
        }

        public async Task<UrlMetaResult> GetUrlMetaAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            string html;
            try
            {
                html = await client.GetStringAsync(url);
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

            // Extract <meta name="description" content="...">
            var descMatch = Regex.Match(html, "<meta[^>]*name=[\"']description[\"'][^>]*content=[\"']([^\"']*)[\"'][^>]*>", RegexOptions.IgnoreCase);
            if (descMatch.Success)
            {
                description = descMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Try <meta content="..." name="description">
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
