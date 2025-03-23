using System;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using SmartReader;

namespace WiseUpDude.Services
{
    public class ContentFetchingService
    {
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;

        public ContentFetchingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _htmlParser = new HtmlParser();
        }

        public async Task<string> FetchTextContentAsync(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL.", nameof(url));

            try
            {
                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync();

                // Use SmartReader to parse out the main article content
                var reader = new Reader(url, htmlContent);
                var article = reader.GetArticle();

                // If article extraction fails, fallback to simple body text
                if (article == null || string.IsNullOrWhiteSpace(article.Content))
                {
                    // fallback to naive approach
                    var document = await _htmlParser.ParseDocumentAsync(htmlContent);
                    var fallback = document.Body?.TextContent ?? string.Empty;
                    return fallback.Trim();
                }

                // article.Content typically returns HTML for the main content
                // If you want plain text, you can strip tags. 
                // SmartReader also has .TextContent but it might be a custom property.
                // For now, let's just return article.Content as is.
                return article.Content;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching/parsing content.", ex);
            }
        }
    }
}
