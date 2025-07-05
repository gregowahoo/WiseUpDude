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

// Assuming WiseUpDude.Model contains your Quiz, Question, and Answer models
namespace WiseUpDude.Services
{
    // Make sure your Quiz, Question, Answer models are correctly defined elsewhere, e.g.:
    // namespace WiseUpDude.Model
    // {
    //     public class Quiz
    //     {
    //         [JsonPropertyName("title")]
    //         public string? Title { get; set; }
    //         [JsonPropertyName("description")]
    //         public string? Description { get; set; }
    //         [JsonPropertyName("questions")]
    //         public List<Question>? Questions { get; set; }
    //         public string? UserId { get; set; }
    //         public string? Prompt { get; set; }
    //         public string? Type { get; set; }
    //         public string? Name { get; set; }
    //         public string? Url { get; set; }
    //         public DateTime CreationDate { get; set; }
    //     }

    //     public class Question
    //     {
    //         [JsonPropertyName("question_text")]
    //         public string? QuestionText { get; set; }
    //         [JsonPropertyName("type")]
    //         public string? Type { get; set; } // e.g., "multiple_choice", "true_false"
    //         [JsonPropertyName("options")]
    //         public List<string>? Options { get; set; } // For multiple choice
    //         [JsonPropertyName("correct_answer")]
    //         public string? CorrectAnswer { get; set; }
    //         [JsonPropertyName("explanation")]
    //         public string? Explanation { get; set; }
    //     }
    // }

    public class GeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeminiService> _logger;
        private readonly UrlMetaService _urlMetaService; // Assuming you still need this for meta data
        private readonly string _geminiApiKey;
        private const string GEMINI_MODEL = "gemini-1.5-flash"; // Or gemini-1.5-pro, or gemini-pro

        public GeminiService(
            IHttpClientFactory httpClientFactory,
            ILogger<GeminiService> logger,
            UrlMetaService urlMetaService,
            IConfiguration configuration) // Inject IConfiguration
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _urlMetaService = urlMetaService;
            _geminiApiKey = configuration["GeminiApiKey"] ?? throw new ArgumentNullException("GeminiApiKey", "Gemini API Key is not configured in appsettings.json or environment variables.");
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromUrlAsync(string url, string? userId)
        {
            // Get the static instructions for quiz generation
            var instructionPrompt = QuizPromptTemplates.BuildQuizGenerationInstructions();

            // Pass the instructions AND the URL to the core API call method
            var (json, apiError) = await GetGeminiQuizJsonAsync(instructionPrompt, url);

            if (apiError != null)
                return (null, apiError);

            var (quizModel, parseError) = ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);

            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Gemini response.");

            // Fetch meta data for the URL (this is done outside the AI API call, similar to your original)
            var meta = await GetUrlMetaAsync(url);
            quizModel.UserId = userId;
            quizModel.Prompt = instructionPrompt; // Store the prompt used
            quizModel.Type = "Url";
            quizModel.Name = meta.Title ?? url;
            quizModel.Description = meta.Description;
            quizModel.Url = url;
            quizModel.CreationDate = DateTime.UtcNow;

            return (quizModel, null);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromPromptAsync(string prompt, string? userId)
        {
            // Get the static instructions for quiz generation
            var instructionPrompt = QuizPromptTemplates.BuildQuizGenerationInstructions();

            // Combine the general instructions with the user's specific text prompt
            // This forms the complete text input for the model
            var combinedPrompt = $"{instructionPrompt}\n\nContent to base the quiz on: {prompt}";

            var (json, apiError) = await GetGeminiQuizJsonAsync(combinedPrompt, null); // No URL for this case

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
            quizModel.Name = prompt; // Name can be the prompt or derived
            quizModel.Description = prompt; // Description can be the prompt or derived
            quizModel.CreationDate = DateTime.UtcNow;

            return (quizModel, null);
        }

        public async Task<(List<string>? Urls, string? Error)> GenerateSuggestedUrlsAsync()
        {
            // Get the static instructions for quiz generation
            var instructionPrompt = QuizPromptTemplates.BuildQuizGenerationInstructions();

            // Append the specific request for URLs to the general instructions
            const string specificRequest = "Generate a list of 20 URLs for web pages that would be suitable for generating a quiz. " +
                                           "Focus on reference-style pages like those from Wikipedia, WebMD, or other reputable sources with dense, informative content. " +
                                           "Avoid homepages, forums, or interactive sites. " +
                                           "Return ONLY the URLs as a JSON array of strings. " +
                                           "For example: [\"https://www.webmd.com/diabetes/type-2-diabetes\", \"https://en.wikipedia.org/wiki/Roman_Empire\", \"https://www.nationalgeographic.com/animals\"]";

            var combinedPrompt = $"{instructionPrompt}\n\n{specificRequest}";

            var (json, apiError) = await GetGeminiQuizJsonAsync(combinedPrompt, null); // No URL input for this call

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
            // Get the static instructions for quiz generation
            var instructionPrompt = QuizPromptTemplates.BuildQuizGenerationInstructions();

            // Append the specific request for prompts to the general instructions
            const string specificRequest = "Generate a list of 20 practical and helpful quiz prompts on a diverse range of topics. " +
                                           "Include topics relevant to different age groups, from young adults to seniors. " +
                                           "The prompts should be suitable for generating a 10-15 question quiz. " +
                                           "Return ONLY the topics as a JSON array of strings. " +
                                           "For example: [\"Effective strategies for managing hot flashes\", \"The long-term effects of alcohol abuse\", \"Beginner's guide to mindfulness\", \"Understanding the basics of Alzheimer's disease\"]";

            var combinedPrompt = $"{instructionPrompt}\n\n{specificRequest}";

            var (json, apiError) = await GetGeminiQuizJsonAsync(combinedPrompt, null); // No URL input for this call

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
                // Add the URL as a specific part for Gemini to fetch and read
                requestParts.Add(new
                {
                    url_data = new
                    {
                        mime_type = "text/html", // Assuming it's a standard web page
                        url = url
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
                    // temperature = 0.7, // Adjust as needed for creativity vs. factualness
                    // topP = 0.95,
                    // topK = 60,
                    responseMimeType = "application/json" // Crucial for getting JSON output
                },
                // tools = new[] { new { Google Search = new object() } } // Only enable if you want it to search broadly
            };

            // Include the API key directly in the URL as a query parameter
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

                // Parse the Gemini response structure
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
                            // Gemini can also return markdown code blocks, just like Perplexity.
                            // We'll apply the same stripping logic.
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

        // Re-use your existing ParseQuizJson and GetUrlMetaAsync methods
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
            // This service is external and remains the same
            return await _urlMetaService.GetUrlMetaAsync(url);
        }
    }
}