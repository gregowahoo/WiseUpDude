using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization; // For JsonStringEnumConverter if needed
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WiseUpDude.Model; // Add this to access config

namespace WiseUpDude.Services
{
    public class GeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeminiService> _logger;
        private readonly UrlMetaService _urlMetaService;
        // Removed ContentFetchingService as per your request
        // private readonly ContentFetchingService _contentFetchingService;
        private readonly string _geminiApiKey;
        private const string GEMINI_MODEL = "gemini-1.5-flash"; // Or gemini-1.5-pro, or gemini-pro

        public GeminiService(
            IHttpClientFactory httpClientFactory,
            ILogger<GeminiService> logger,
            UrlMetaService urlMetaService,
            // Removed ContentFetchingService from constructor as per your request
            // ContentFetchingService contentFetchingService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _urlMetaService = urlMetaService;
            // Removed ContentFetchingService assignment as per your request
            // _contentFetchingService = contentFetchingService;
            _geminiApiKey = configuration["GeminiApiKey"] ?? throw new ArgumentNullException("GeminiApiKey", "Gemini API Key is not configured in appsettings.json or environment variables.");
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromUrlAsync(string url, string? userId)
        {
            // Get the static instructions for quiz generation
            var instructionPrompt = QuizPromptTemplates.BuildQuizGenerationInstructions();

            // Pass the instructions AND the URL to the core API call method
            // Gemini is expected to fetch the URL content directly via uri_data
            var (json, apiError) = await GetGeminiQuizJsonAsync(instructionPrompt, url);

            if (apiError != null)
                return (null, apiError);

            var (quizModel, parseError) = ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);

            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Gemini response.");

            var meta = await GetUrlMetaAsync(url);
            quizModel.UserId = userId;
            quizModel.Prompt = instructionPrompt;
            quizModel.Type = "Url";
            quizModel.Name = meta.Title ?? url;
            quizModel.Description = meta.Description;
            quizModel.Url = url;
            quizModel.CreationDate = DateTime.UtcNow;

            return (quizModel, null);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromPromptAsync(string prompt, string? userId)
        {
            var instructionPrompt = QuizPromptTemplates.BuildQuizGenerationInstructions();
            var combinedPrompt = $"{instructionPrompt}\n\nContent to base the quiz on: {prompt}";

            var (json, apiError) = await GetGeminiQuizJsonAsync(combinedPrompt, null);

            if (apiError != null)
                return (null, apiError);

            var (quizModel, parseError) = ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);

            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Gemini response.");

            quizModel.UserId = userId;
            quizModel.Prompt = prompt;
            quizModel.Type = "Prompt";
            quizModel.Name = prompt;
            quizModel.Description = prompt;
            quizModel.CreationDate = DateTime.UtcNow;

            return (quizModel, null);
        }

        // Internal method to handle the actual API call to Gemini
        private async Task<(string? Json, string? Error)> GetGeminiQuizJsonAsync(string userPrompt, string? url = null)
        {
            var client = _httpClientFactory.CreateClient("GeminiAI");

            var requestParts = new List<object>
            {
                new { text = userPrompt }
            };

            if (!string.IsNullOrEmpty(url))
            {
                // CORRECTED BACK TO uri_data and uri as per standard documentation for web URLs
                requestParts.Add(new
                {
                    uri_data = new // Back to uri_data
                    {
                        mime_type = "text/html", // Assuming it's a standard web page
                        uri = url // Back to uri
                    }
                });
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new { role = "user", parts = requestParts.ToArray() }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            var requestUri = $"v1beta/models/{GEMINI_MODEL}:generateContent?key={_geminiApiKey}";

            _logger.LogInformation("Sending request to Gemini API. URL: {Url}, Body: {Body}",
                client.BaseAddress + requestUri,
                System.Text.Json.JsonSerializer.Serialize(requestBody));

            HttpResponseMessage geminiResponse;
            try
            {
                geminiResponse = await client.PostAsJsonAsync(requestUri, requestBody);
                _logger.LogInformation("Gemini API response status: {StatusCode}", geminiResponse.StatusCode);

                if (!geminiResponse.IsSuccessStatusCode)
                {
                    var errorContent = await geminiResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Gemini API error response: {Content}", errorContent);
                    return (null, $"Gemini API error: {geminiResponse.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling Gemini API");
                return (null, $"Gemini API error: {ex.Message}");
            }

            string jsonContent;
            try
            {
                var geminiContent = await geminiResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("Gemini API raw response: {Content}", geminiContent);

                using var doc = JsonDocument.Parse(geminiContent);
                var candidates = doc.RootElement.GetProperty("candidates");
                if (candidates.ValueKind == JsonValueKind.Array && candidates.EnumerateArray().Any())
                {
                    var firstCandidate = candidates.EnumerateArray().First();
                    if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                        contentElement.TryGetProperty("parts", out var partsElement) &&
                        partsElement.ValueKind == JsonValueKind.Array && partsElement.EnumerateArray().Any())
                    {
                        var firstPart = partsElement.EnumerateArray().First();
                        if (firstPart.TryGetProperty("text", out var textElement))
                        {
                            jsonContent = textElement.GetString() ?? "";
                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                jsonContent = jsonContent.Trim();
                                if (jsonContent.StartsWith("```"))
                                {
                                    int firstNewline = jsonContent.IndexOf('\n');
                                    if (firstNewline > 0)
                                    {
                                        jsonContent = jsonContent.Substring(firstNewline + 1);
                                    }
                                    if (jsonContent.EndsWith("```"))
                                    {
                                        jsonContent = jsonContent.Substring(0, jsonContent.Length - 3).TrimEnd();
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Gemini API response part did not contain 'text' property.");
                            return (null, "Gemini API response part did not contain expected text content.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Gemini API response candidate did not contain 'content.parts'.");
                        return (null, "Gemini API response did not contain expected content parts.");
                    }
                }
                else
                {
                    _logger.LogWarning("Gemini API response did not contain any candidates.");
                    return (null, "Gemini API did not return any content candidates.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response JSON structure.");
                return (null, $"Failed to parse Gemini API response: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(jsonContent) || !(jsonContent.TrimStart().StartsWith("{") || jsonContent.TrimStart().StartsWith("[")))
            {
                _logger.LogWarning("Gemini did not return valid JSON. Raw response content: {Raw}", jsonContent);
                return (null, $"Gemini did not return valid JSON. Raw response: {jsonContent}");
            }

            return (jsonContent, null);
        }

        public async Task<(List<string>? Urls, string? Error)> GenerateSuggestedUrlsAsync()
        {
            const string aiPrompt = "Generate a list of 20 URLs for web pages that would be suitable for generating a quiz. " +
                                    "Focus on reference-style pages like those from Wikipedia, WebMD, or other reputable sources with dense, informative content. " +
                                    "Avoid homepages, forums, or interactive sites. " +
                                    "Return the URLs as a JSON array of strings. " +
                                    "For example: [\"https://www.webmd.com/diabetes/type-2-diabetes\", \"https://en.wikipedia.org/wiki/Roman_Empire\", \"https://www.nationalgeographic.com/animals\"]";

            var (json, apiError) = await GetGeminiQuizJsonAsync(aiPrompt, null);

            if (apiError != null)
            {
                return (null, apiError);
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var urls = JsonSerializer.Deserialize<List<string>>(json, options);
                return (urls, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse suggested URLs JSON from Gemini. Raw response: {json}", json);
                return (null, $"Failed to parse suggested URLs JSON: {ex.Message}.");
            }
        }

        public async Task<(List<string>? Prompts, string? Error)> GenerateSuggestedPromptsAsync()
        {
            const string aiPrompt = "Generate a list of 20 practical and helpful quiz prompts on a diverse range of topics. " +
                                    "Include topics relevant to different age groups, from young adults to seniors. " +
                                    "The prompts should be suitable for generating a 10-15 question quiz. " +
                                    "Return the topics as a JSON array of strings. " +
                                    "For example: [\"Effective strategies for managing hot flashes\", \"The long-term effects of alcohol abuse\", \"Beginner's guide to mindfulness\", \"Understanding the basics of Alzheimer's disease\"]";

            var (json, apiError) = await GetGeminiQuizJsonAsync(aiPrompt, null);

            if (apiError != null)
            {
                return (null, apiError);
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var prompts = JsonSerializer.Deserialize<List<string>>(json, options);
                return (prompts, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse suggested prompts JSON from Gemini. Raw response: {json}", json);
                return (null, $"Failed to parse suggested prompts JSON: {ex.Message}.");
            }
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