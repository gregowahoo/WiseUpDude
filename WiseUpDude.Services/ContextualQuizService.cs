using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class ContextualQuizService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ContextualQuizService> _logger;

        public ContextualQuizService(
            IHttpClientFactory httpClientFactory,
            ILogger<ContextualQuizService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizWithContextAsync(
            string contextSource, // URL or prompt topic
            string? explicitContextSummary,
            string searchContextSize = "medium")
        {
            var promptBody = ContextualQuizPromptTemplates.BuildQuizPromptWithContext(
                contextSource, explicitContextSummary);

            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                model = "sonar",
                search_context_size = searchContextSize,
                messages = new[]
                {
                    new { role = "system", content =
                        "You are a contextual quiz generator. For each quiz question, always provide a 1-2 sentence summary of why the question is relevant, before listing answer options. Context must be presented as a field in the JSON output. Base all questions, answers, and explanations only on the provided content and context." },
                    //new { role = "user", content = explicitContextSummary == null ? "" : $"Context: {explicitContextSummary}" },
                    //new { role = "user", content = promptBody }
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
            // Assume downstream logic parses JSON and maps to Quiz model
            var (quiz, parseError) = ParseQuizJson(content);
            return (quiz, parseError);
        }

        private (Quiz? Quiz, string? Error) ParseQuizJson(string json)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<PerplexityApiResponse>(json, options);

                // Adjust this line to match your actual Choice/message structure
                var quizJson = apiResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrWhiteSpace(quizJson))
                    return (null, "No quiz content found in API response.");

                var quiz = System.Text.Json.JsonSerializer.Deserialize<Quiz>(quizJson, options);
                return (quiz, null);
            }
            catch (Exception ex)
            {
                return (null, $"Failed to parse quiz JSON: {ex.Message}. Raw response: {json}");
            }
        }
    }
}
