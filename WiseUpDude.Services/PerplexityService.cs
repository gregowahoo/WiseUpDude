using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WiseUpDude.Model;
using WiseUpDude.Shared.Model;
using WiseUpDude.Shared.Services;

namespace WiseUpDude.Services
{
    public class PerplexityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ContentFetchingService _contentFetchingService;
        private readonly ILogger<PerplexityService> _logger;
        private readonly UrlMetaService _urlMetaService;

        public PerplexityService(
            IHttpClientFactory httpClientFactory,
            ContentFetchingService contentFetchingService,
            ILogger<PerplexityService> logger,
            UrlMetaService urlMetaService)
        {
            _httpClientFactory = httpClientFactory;
            _contentFetchingService = contentFetchingService;
            _logger = logger;
            _urlMetaService = urlMetaService;
        }

        // Add: GenerateQuizFromUserInputAsync for user quizzes (not LearningTrack)
        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromUrlAsync(string url, string? userId)
        {
            // Append timestamp to force a fresh fetch
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var urlWithTimestamp = url.Contains("?")
                ? $"{url}&t={timestamp}"
                : $"{url}?t={timestamp}";

            //var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(urlWithTimestamp);
            // Use the URL directly for the prompt
            var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(url);

            var (json, apiError) = await GetPerplexityQuizJsonAsync(aiPrompt);

            if (apiError != null)
                return (null, apiError);
            var (quizModel, parseError) = ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);
            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Perplexity response.");

            // Fetch meta data for the URL
            var meta = await GetUrlMetaAsync(url);
            quizModel.UserId = userId;
            quizModel.Prompt = string.Empty;
            quizModel.Type = "Url";
            quizModel.Name = meta.Title ?? url;
            quizModel.Description = meta.Description ?? meta.Title;
            quizModel.Url = url;
            quizModel.CreationDate = DateTime.UtcNow;

            return (quizModel, null);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromPromptAsync(string prompt, string? userId)
        {
            var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(prompt);
            var (json, apiError) = await GetPerplexityQuizJsonAsync(aiPrompt);

            if (apiError != null)
                return (null, apiError);
            var (quizModel, parseError) = ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);
            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Perplexity response.");

            // Fetch meta data for the URL
            //var meta = await GetUrlMetaAsync(prompt);
            quizModel.UserId = userId;
            quizModel.Prompt = prompt;
            quizModel.Type = "Prompt";
            quizModel.Name = prompt;
            quizModel.Description = prompt;
            quizModel.CreationDate = DateTime.UtcNow;

            return (quizModel, null);
        }

        public async Task<(List<string>? Urls, string? Error)> GenerateSuggestedUrlsAsync(string? theme)
        {
            string aiPrompt = string.IsNullOrWhiteSpace(theme)
                ? "Generate a list of 20 URLs for web pages that would be suitable for generating a quiz. " +
                  "Focus on reference-style pages like those from Wikipedia, WebMD, or other reputable sources with dense, informative content. " +
                  "Avoid homepages, forums, or interactive sites. " +
                  "Return only the URLs as a JSON array of strings, with no explanation, markdown, or formatting. " +
                  "For example: [\"https://www.webmd.com/diabetes/type-2-diabetes\", \"https://en.wikipedia.org/wiki/Roman_Empire\", \"https://www.nationalgeographic.com/animals\"]"
                : $"Generate a list of 20 URLs for web pages about '{theme}' that would be suitable for generating a quiz. " +
                  "Focus on reference-style pages like those from Wikipedia, WebMD, or other reputable sources with dense, informative content. " +
                  "Avoid homepages, forums, or interactive sites. " +
                  "Return only the URLs as a JSON array of strings, with no explanation, markdown, or formatting.";

            var (json, apiError) = await GetPerplexityQuizJsonAsync(aiPrompt);

            if (apiError != null)
                return (null, apiError);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var urls = JsonSerializer.Deserialize<List<string>>(json, options);
                return (urls, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse suggested URLs JSON. Raw response: {json}", json);
                return (null, $"Failed to parse suggested URLs JSON: {ex.Message}.");
            }
        }

        public async Task<(List<string>? Prompts, string? Error)> GenerateSuggestedPromptsAsync(string? theme)
        {
            string aiPrompt = string.IsNullOrWhiteSpace(theme)
                ? "Generate a list of 20 practical and helpful quiz prompts on a diverse range of topics. " +
                  "Include topics relevant to different age groups, from young adults to seniors. " +
                  "The prompts should be suitable for generating a 10-15 question quiz. " +
                  "Return only the topics as a JSON array of strings, with no explanation, markdown, or formatting. " +
                  "For example: [\"Effective strategies for managing hot flashes\", \"The long-term effects of alcohol abuse\", \"Beginner's guide to mindfulness\", \"Understanding the basics of Alzheimer's disease\"]"
                : $"Generate a list of 20 practical and helpful quiz prompts on the theme '{theme}'. " +
                  "The prompts should be suitable for generating a 10-15 question quiz. " +
                  "Return only the topics as a JSON array of strings, with no explanation, markdown, or formatting.";

            var (json, apiError) = await GetPerplexityQuizJsonAsync(aiPrompt);

            if (apiError != null)
                return (null, apiError);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var prompts = JsonSerializer.Deserialize<List<string>>(json, options);
                return (prompts, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse suggested prompts JSON. Raw response: {json}", json);
                return (null, $"Failed to parse suggested prompts JSON: {ex.Message}.");
            }
        }

        // Make these public for LearningTrackQuizService
        public async Task<(string? Json, string? Error)> GetPerplexityQuizJsonAsync(string aiPrompt)
        {
            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                //model = "sonar-pro",
                model = "sonar",
                //model = "sonar-deep-research",
                //model = "sonar-reasoning-pro",
                //model = "sonar-reasoning",
                messages = new[]
                {
                    new { role = "user", content = aiPrompt }
                }
            };
            _logger.LogInformation("Sending request to Perplexity API. URL: {Url}, Headers: {Headers}, Body: {Body}",
                client.BaseAddress + "/chat/completions",
                string.Join(", ", client.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(";", h.Value)}")),
                System.Text.Json.JsonSerializer.Serialize(requestBody));
            HttpResponseMessage perplexityResponse;
            try
            {
                perplexityResponse = await client.PostAsJsonAsync("/chat/completions", requestBody);
                _logger.LogInformation("Perplexity API response status: {StatusCode}", perplexityResponse.StatusCode);
                if (!perplexityResponse.IsSuccessStatusCode)
                {
                    var errorContent = await perplexityResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Perplexity API error response: {Content}", errorContent);
                    return (null, $"Perplexity API error: {perplexityResponse.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling Perplexity API");
                return (null, $"Perplexity API error: {ex.Message}");
            }
            string json;
            try
            {
                var perplexityContent = await perplexityResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("Perplexity API raw response: {Content}", perplexityContent);
                var doc = JsonDocument.Parse(perplexityContent);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content").GetString();
                json = content ?? "";
                if (!string.IsNullOrWhiteSpace(json))
                {
                    json = json.Trim();
                    if (json.StartsWith("```"))
                    {
                        int firstNewline = json.IndexOf('\n');
                        if (firstNewline > 0)
                        {
                            json = json.Substring(firstNewline + 1);
                        }
                        if (json.EndsWith("```"))
                        {
                            json = json.Substring(0, json.Length - 3).TrimEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Perplexity API response");
                return (null, $"Failed to parse Perplexity API response: {ex.Message}");
            }
            if (string.IsNullOrWhiteSpace(json) || !(json.TrimStart().StartsWith("{") || json.TrimStart().StartsWith("[")))
            {
                _logger.LogWarning("Perplexity did not return valid JSON. Raw response: {Raw}", json);
                return (null, $"Perplexity did not return valid JSON. Raw response: {json}");
            }
            return (json, null);
        }

        public (Quiz? QuizModel, string? Error) ParseQuizJson(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter());
                var quizModel = JsonSerializer.Deserialize<Quiz>(json, options);
                return (quizModel, null);
            }
            catch (Exception ex)
            {
                return (null, $"Failed to parse quiz JSON: {ex.Message}. Raw response: {json}");
            }
        }

        public async Task<UrlMetaResult> GetUrlMetaAsync(string url)
        {
            return await _urlMetaService.GetUrlMetaAsync(url);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizWithContextFromPromptAsync(string prompt, string? userId)
        {
            var explicitContextSummary = "Summary of key points from the content or user-provided background.";
            var searchContextSize = "medium"; // Default size, can be adjusted as needed

            var promptBody = ContextualQuizPromptTemplates.BuildQuizPromptWithContext(prompt, explicitContextSummary);

            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                model = "sonar",
                search_context_size = searchContextSize,
                messages = new[]
                {
                    new { role = "system", content = "You are a contextual quiz generator. For each quiz question, always provide a 1-2 sentence summary of why the question is relevant, before listing answer options. Context must be presented as a field in the JSON output. Base all questions, answers, and explanations only on the provided content and context." },
                    new { role = "user", content = $"Context: {explicitContextSummary}\n\n{promptBody}" }
                }
            };

            _logger.LogInformation("Calling Perplexity API for contextual quiz.");

            var response = await client.PostAsJsonAsync("/chat/completions", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, $"API Error: {response.StatusCode}: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var (quiz, parseError) = ParseContextualQuizJson(content);


            if (quiz != null)
            {
                if (quiz.Questions == null || !quiz.Questions.Any())
                    return (null, "No quiz questions found in Perplexity response.");
                // Set additional properties for the quiz
                quiz.UserId = userId;
                quiz.Type = "Prompt";
                quiz.Prompt = prompt;
                quiz.Name = prompt;
                quiz.Description = prompt;
                quiz.CreationDate = DateTime.UtcNow;
            }

            return (quiz, parseError);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizWithContextFromUrlAsync(string url, string? userId)
        {
            var explicitContextSummary = "Summary of key points from the content or user-provided background.";
            var searchContextSize = "medium"; // Default size, can be adjusted as needed

            var promptBody = ContextualQuizPromptTemplates.BuildQuizPromptWithContext(url, explicitContextSummary);

            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                model = "sonar",
                search_context_size = searchContextSize,
                messages = new[]
                {
                    new { role = "system", content = "You are a contextual quiz generator. For each quiz question, always provide a 1-2 sentence summary of why the question is relevant, before listing answer options. Context must be presented as a field in the JSON output. Base all questions, answers, and explanations only on the provided content and context." },
                    new { role = "user", content = $"Context: {explicitContextSummary}\n\n{promptBody}" }
                }
            };

            _logger.LogInformation("Calling Perplexity API for contextual quiz.");

            var response = await client.PostAsJsonAsync("/chat/completions", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (null, $"API Error: {response.StatusCode}: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var (quiz, parseError) = ParseContextualQuizJson(content);

            if (quiz != null)
            {
                if (quiz.Questions == null || !quiz.Questions.Any())
                    return (null, "No quiz questions found in Perplexity response.");

                // Set additional properties for the quiz
                var meta = await GetUrlMetaAsync(url);                                      // Fetch meta data for the URL

                quiz.UserId = userId;
                quiz.Prompt = string.Empty;
                quiz.Type = "Url";
                quiz.Name = meta.Title ?? url;
                quiz.Description = meta.Description ?? meta.Title;
                quiz.Url = url;
                quiz.CreationDate = DateTime.UtcNow;
            }

            return (quiz, parseError);
        }
        private (Quiz? Quiz, string? Error) ParseContextualQuizJson(string json)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
                };
                bool truncated;
                var cleanedJson = CleanJsonUtility.CleanJson(json, out truncated);
                if (truncated)
                {
                    // Log a warning or handle as needed
                }
                var apiResponse = JsonSerializer.Deserialize<PerplexityApiResponse>(cleanedJson, options);
                var quizJson = apiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrWhiteSpace(quizJson))
                    return (null, "No quiz content found in API response.");
                
                var cleanedQuizJson = CleanJsonUtility.CleanJson(quizJson, out truncated);

                var quiz = System.Text.Json.JsonSerializer.Deserialize<Quiz>(cleanedQuizJson, options);
                if (quiz == null)
                    return (null, "Failed to deserialize quiz from API response."); 

                return (quiz, null);
            }
            catch (Exception ex)
            {
                //return (null, $"Failed to parse quiz JSON: {ex.Message}. Raw response: {json}");
                return (null, $"Failed to parse quiz JSON: {ex.Message}. Please try again.");
            }
        }
    }
}