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
        private readonly AnswerRandomizerService _answerRandomizer;

        public PerplexityService(
            IHttpClientFactory httpClientFactory,
            ContentFetchingService contentFetchingService,
            ILogger<PerplexityService> logger,
            UrlMetaService urlMetaService,
            AnswerRandomizerService answerRandomizer)
        {
            _httpClientFactory = httpClientFactory;
            _contentFetchingService = contentFetchingService;
            _logger = logger;
            _urlMetaService = urlMetaService;
            _answerRandomizer = answerRandomizer;
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
            var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(url, _logger);

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

            // Apply answer randomization to ensure even distribution across positions
            //quizModel = _answerRandomizer.DistributeAnswersEvenly(quizModel);

            return (quizModel, null);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromPromptAsync(string prompt, string? userId)
        {
            var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(prompt, _logger);
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

            // Apply answer randomization to ensure even distribution across positions
            //quizModel = _answerRandomizer.DistributeAnswersEvenly(quizModel);

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

        private static List<CitationMeta> ConvertPerplexityCitations(object? citationField)
        {
            if (citationField == null)
                return new List<CitationMeta>();

            if (citationField is List<CitationMeta> metas)
                return metas;

            if (citationField is List<string> urls)
                return urls.Select(url => new CitationMeta { Url = url }).ToList();

            if (citationField is string citationJson)
            {
                try
                {
                    var citationUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(citationJson);
                    return citationUrls?.Select(url => new CitationMeta { Url = url }).ToList() ?? new List<CitationMeta>();
                }
                catch
                {
                    return new List<CitationMeta>();
                }
            }

            return new List<CitationMeta>();
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

                // Convert citations for each question
                if (quizModel?.Questions != null)
                {
                    foreach (var question in quizModel.Questions)
                    {
                        question.Citation = ConvertPerplexityCitations(question.Citation);
                    }
                }

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

            var promptBody = ContextualQuizPromptTemplates.BuildQuizPromptWithUserTopic(prompt, explicitContextSummary, _logger);

            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                //model = "sonar",
                //model = "sonar-pro-chat",
                model = "sonar-pro",
                search_context_size = searchContextSize,
                messages = new[]
                {
                    new { role = "system", 
                    content = @"You are a contextual quiz generator.
For each quiz question:
- Always provide a 1-2 sentence summary ('ContextSnippet') of why the question is relevant, before listing answer options.
- Context must be presented as a field in the JSON output.

When generating answers and explanations:
- The explanation MUST always fully support and justify the provided answer. You must check and double-check that the 'Answer' and 'Explanation' fields are perfectly consistent and logically aligned.
- For True/False questions, never allow a mismatch: the explanation must clearly justify why the answer is 'True' or 'False' for the question as stated.
- If you find any mismatch between answer and explanation, immediately regenerate the pair so they agree.
- Base all questions, answers, and explanations ONLY on the provided content and context." },

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

                // Fetch meta data for each citation in each question
                foreach (var question in quiz.Questions)
                {
                    if (question.Citation != null)
                    {
                        for (int i = 0; i < question.Citation.Count; i++)
                        {
                            var citation = question.Citation[i];
                            if (!string.IsNullOrWhiteSpace(citation.Url) && IsValidUrl(citation.Url))
                            {
                                var meta = await GetUrlMetaAsync(citation.Url);
                                citation.Title = meta.Title;
                                citation.Description = meta.Description;
                                question.Citation[i] = citation;
                            }
                        }
                        // Remove citations with no info
                        question.Citation = question.Citation
                            .Where(c => !(string.IsNullOrWhiteSpace(c.Title) || c.Title == c.Url) && !string.IsNullOrWhiteSpace(c.Description))
                            .ToList();
                    }
                    // Verification for True/False questions (refactored)
                    await VerifyAndFixTrueFalseQuestionAsync(question);
                }

                // Apply answer randomization to ensure even distribution across positions
                quiz = _answerRandomizer.DistributeAnswersEvenly(quiz);
            }

            return (quiz, parseError);
        }

        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizWithContextFromUrlAsync(string url, string? userId)
        {
            var explicitContextSummary = "Summary of key points from the content or user-provided background.";
            var searchContextSize = "medium"; // Default size, can be adjusted as needed

            //var promptBody = ContextualQuizPromptTemplates.BuildQuizPromptWithUserTopic(url, explicitContextSummary, _logger);
            var promptBody = ContextualQuizPromptTemplates.BuildQuizPromptWithUrlContext(url, explicitContextSummary, _logger);

            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                //model = "sonar",
                //model = "sonar-pro-chat",
                model = "sonar-pro",
                search_context_size = searchContextSize,
                messages = new[]
                {
                    new { role = "system", 
                        //content = "You are a contextual quiz generator. For each quiz question, always provide a 1-2 sentence summary of why the question is relevant, before listing answer options. Context must be presented as a field in the JSON output. Base all questions, answers, and explanations only on the provided content and context." },
                    content = @"You are a contextual quiz generator.
For each quiz question:
- Always provide a 1-2 sentence summary ('ContextSnippet') of why the question is relevant, before listing answer options.
- Context must be presented as a field in the JSON output.

When generating answers and explanations:
- The explanation MUST always fully support and justify the provided answer. You must check and double-check that the 'Answer' and 'Explanation' fields are perfectly consistent and logically aligned.
- For True/False questions, never allow a mismatch: the explanation must clearly justify why the answer is 'True' or 'False' for the question as stated.
- If you find any mismatch between answer and explanation, immediately regenerate the pair so they agree.
- Base all questions, answers, and explanations ONLY on the provided content and context." },

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

                // Do NOT fetch meta data for the original URL
                quiz.UserId = userId;
                quiz.Prompt = string.Empty;
                quiz.Type = "Url";
                quiz.Name = url;
                quiz.Description = url;
                quiz.Url = url;
                quiz.CreationDate = DateTime.UtcNow;

                // Fetch meta data for each citation in each question
                foreach (var question in quiz.Questions)
                {
                    if (question.Citation != null)
                    {
                        for (int i = 0; i < question.Citation.Count; i++)
                        {
                            var citation = question.Citation[i];
                            if (!string.IsNullOrWhiteSpace(citation.Url) && IsValidUrl(citation.Url))
                            {
                                var meta = await GetUrlMetaAsync(citation.Url);
                                citation.Title = meta.Title;
                                citation.Description = meta.Description;
                                question.Citation[i] = citation;
                            }
                        }
                        // Remove citations with no info
                        question.Citation = question.Citation
                            .Where(c => !(string.IsNullOrWhiteSpace(c.Title) || c.Title == c.Url) && !string.IsNullOrWhiteSpace(c.Description))
                            .ToList();
                    }
                    // Verification for True/False questions (refactored)
                    //await VerifyAndFixTrueFalseQuestionAsync(question);
                }

                // Apply answer randomization to ensure even distribution across positions
                quiz = _answerRandomizer.DistributeAnswersEvenly(quiz);
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
                    Converters = {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false),
                        new CitationMetaListConverter() // <-- Register the custom converter
                    }
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

                // Convert citations for each question (optional, but now should be safe)
                if (quiz.Questions != null)
                {
                    foreach (var question in quiz.Questions)
                    {
                        question.Citation = question.Citation ?? new List<CitationMeta>();
                    }
                }

                return (quiz, null);
            }
            catch (Exception ex)
            {
                return (null, $"Failed to parse quiz JSON: {ex.Message}. Please try again.");
            }
        }

        private async Task<bool> VerifyTrueFalseAnswerAsync(string question, string answer, string explanation)
        {
            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var prompt = $@"Given the following True/False question, answer, and explanation, does the explanation fully justify and support the answer? Reply only 'Yes' or 'No'.\n\nQuestion: {question}\nAnswer: {answer}\nExplanation: {explanation}";
            var requestBody = new
            {
                model = "sonar-pro",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };
            var response = await client.PostAsJsonAsync("/chat/completions", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[TF_VERIFY] Verification API call failed for question: {Question}", question);
                return true; // Fail open: don't flip if verification fails
            }
            var content = await response.Content.ReadAsStringAsync();
            bool isValid = content.Contains("Yes", StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation("[TF_VERIFY] Q: {Question} | A: {Answer} | Explanation: {Explanation} | API Response: {ApiResponse} | IsValid: {IsValid}", question, answer, explanation, content, isValid);
            return isValid;
        }

        private async Task VerifyAndFixTrueFalseQuestionAsync(QuizQuestion question)
        {
            if (question.QuestionType != QuizQuestionType.TrueFalse ||
                string.IsNullOrWhiteSpace(question.Question) ||
                string.IsNullOrWhiteSpace(question.Answer) ||
                string.IsNullOrWhiteSpace(question.Explanation))
                return;

            string originalAnswer = question.Answer?.Trim();
            bool isValid = await VerifyTrueFalseAnswerAsync(question.Question, question.Answer, question.Explanation);
            if (!isValid)
            {
                string normalized = (originalAnswer ?? string.Empty).Trim().ToLowerInvariant();
                string flipped;
                if (normalized == "true")
                    flipped = "False";
                else if (normalized == "false")
                    flipped = "True";
                else
                {
                    _logger.LogWarning("[TF_VERIFY_FLIP] Could not flip answer for Q: {Question} | Unrecognized answer: {Answer}", question.Question, question.Answer);
                    return;
                }
                _logger.LogWarning("[TF_VERIFY_FLIP] Flipping answer for Q: {Question} | Old: {OldAnswer} | New: {NewAnswer} | Explanation: {Explanation}", question.Question, originalAnswer, flipped, question.Explanation);
                question.Answer = flipped;
            }
            else
            {
                _logger.LogInformation("[TF_VERIFY_OK] Q: {Question} | Answer: {Answer} | Explanation: {Explanation} | No flip needed.", question.Question, question.Answer, question.Explanation);
            }
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }

    public class CitationMetaListConverter : JsonConverter<List<CitationMeta>>
    {
        public override List<CitationMeta> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<CitationMeta>();

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return result;
                throw new JsonException("Expected StartArray token for Citation field.");
            }

            // Read each element in the array
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.String)
                {
                    result.Add(new CitationMeta { Url = reader.GetString() });
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Deserialize the object directly
                    var obj = JsonSerializer.Deserialize<CitationMeta>(ref reader, options);
                    if (obj != null)
                        result.Add(obj);
                }
                else
                {
                    throw new JsonException($"Unexpected token type in Citation array: {reader.TokenType}");
                }
            }

            // Do NOT call reader.Read() here; the reader is already at the correct position
            return result;
        }

        public override void Write(Utf8JsonWriter writer, List<CitationMeta> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}