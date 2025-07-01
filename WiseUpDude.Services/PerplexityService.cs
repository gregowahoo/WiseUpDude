using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WiseUpDude.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
            quizModel.Description = meta.Description;
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

        // Make these public for LearningTrackQuizService
        public async Task<(string? Json, string? Error)> GetPerplexityQuizJsonAsync(string aiPrompt)
        {
            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                model = "sonar-pro",
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

        public async Task<UrlMetaService.UrlMetaResult> GetUrlMetaAsync(string url)
        {
            return await _urlMetaService.GetUrlMetaAsync(url);
        }
    }
}
