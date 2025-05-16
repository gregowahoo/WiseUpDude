using System;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using SmartReader;

namespace WiseUpDude.Services
{
    public class ContentFetchingResult
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ContentFetchingService
    {
        private readonly HttpClient _httpClient;
        private readonly HtmlParser _htmlParser;
        private const int MinimumContentLength = 500; // Adjust as needed

        public ContentFetchingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _htmlParser = new HtmlParser();
        }

        /// <summary>
        /// Fetches and validates text content from the given URL.
        /// Returns a ContentFetchingResult that contains the extracted content or an error message.
        /// </summary>
        /// <param name="url">The URL to fetch and validate content from.</param>
        /// <returns>A ContentFetchingResult with success flag, content, and error message if any.</returns>
        public async Task<ContentFetchingResult> FetchValidatedTextContentAsync(string url)
        {
            var result = new ContentFetchingResult();

            // Validate URL format
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Invalid URL.";
                return result;
            }

            try
            {
                // Fetch the content from the URL
                var response = await _httpClient.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"The URL returned status code {response.StatusCode}.";
                    return result;
                }

                // Check that the response content type indicates HTML
                if (response.Content.Headers.ContentType?.MediaType == null ||
                    !response.Content.Headers.ContentType.MediaType.Contains("html"))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "The URL does not point to an HTML page.";
                    return result;
                }

                var htmlContent = await response.Content.ReadAsStringAsync();

                // Use SmartReader to extract the main article content
                var reader = new Reader(url, htmlContent);
                var article = reader.GetArticle();
                string textContent;

                // Fallback to naive extraction using AngleSharp if SmartReader fails
                if (article == null || string.IsNullOrWhiteSpace(article.Content))
                {
                    var document = await _htmlParser.ParseDocumentAsync(htmlContent);
                    textContent = document.Body?.TextContent ?? string.Empty;
                }
                else
                {
                    textContent = article.Content;
                }

                textContent = textContent.Trim();

                // Validate that there is a sufficient amount of textual content
                if (string.IsNullOrWhiteSpace(textContent) || textContent.Length < MinimumContentLength)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "The page does not contain enough textual content for quiz generation.";
                    return result;
                }

                result.IsSuccess = true;
                result.Content = textContent;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Error fetching/parsing content: " + ex.Message;
                return result;
            }
        }
    }
}
