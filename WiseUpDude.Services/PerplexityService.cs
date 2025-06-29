using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories.Interfaces;
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
        private readonly ILearningTrackQuizRepository _quizRepository;
        private readonly ILearningTrackQuizQuestionRepository _questionRepository;
        private readonly ContentFetchingService _contentFetchingService;
        private readonly ILearningTrackSourceRepository _sourceRepository;
        private readonly ILogger<PerplexityService> _logger;

        public PerplexityService(
            IHttpClientFactory httpClientFactory,
            ILearningTrackQuizRepository quizRepository,
            ILearningTrackQuizQuestionRepository questionRepository,
            ContentFetchingService contentFetchingService,
            ILearningTrackSourceRepository sourceRepository,
            ILogger<PerplexityService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _contentFetchingService = contentFetchingService;
            _sourceRepository = sourceRepository;
            _logger = logger;
        }

        public async Task<(LearningTrackQuiz? Quiz, string? Error)> GenerateAndPersistQuizFromUrlAsync(string url, int learningTrackSourceId)
        {
            var aiPrompt = BuildQuizPrompt(url);
            var sourceName = await GetSourceNameAsync(learningTrackSourceId);
            var (json, apiError) = await GetPerplexityQuizJsonAsync(aiPrompt);
            if (apiError != null)
                return (null, apiError);
            var (quizModel, parseError) = ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);
            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Perplexity response.");
            var quiz = await CreateAndPersistLearningTrackQuiz(quizModel, sourceName, learningTrackSourceId);
            return (quiz, null);
        }

        // Add: GenerateQuizFromUrlAsync for user quizzes (not LearningTrack)
        public async Task<(Quiz? Quiz, string? Error)> GenerateQuizFromUrlAsync(string url, string? userId)
        {
            var aiPrompt = BuildQuizPrompt(url);
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

        private async Task<string> GetSourceNameAsync(int learningTrackSourceId)
        {
            var source = await _sourceRepository.GetByIdAsync(learningTrackSourceId);
            return source?.Name ?? "Unknown Source";
        }

        private async Task<(string? Json, string? Error)> GetPerplexityQuizJsonAsync(string aiPrompt)
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

        private (Quiz? QuizModel, string? Error) ParseQuizJson(string json)
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

        private async Task<LearningTrackQuiz> CreateAndPersistLearningTrackQuiz(Quiz quizModel, string sourceName, int learningTrackSourceId)
        {
            var quiz = new LearningTrackQuiz
            {
                Name = sourceName,
                Description = quizModel.Description,
                LearningTrackSourceId = learningTrackSourceId,
                CreationDate = DateTime.UtcNow,
                Questions = quizModel.Questions.Select(q => new LearningTrackQuizQuestion
                {
                    Question = q.Question ?? string.Empty,
                    Answer = q.Answer ?? string.Empty,
                    Explanation = q.Explanation,
                    OptionsJson = q.OptionsJson,
                    Difficulty = q.Difficulty,
                    CreationDate = DateTime.UtcNow
                }).ToList()
            };
            await _quizRepository.AddQuizAsync(quiz);
            return quiz;
        }

        private string BuildQuizPrompt(string url)
        {
            var prompt = """
Create a quiz based on the following prompt: "Use this URL: {0}"
The quiz should include as many questions as possible, up to a maximum of 25.
Include both multiple-choice and true/false questions.

QUESTION FORMATTING & ANSWER SHUFFLING:
For multiple-choice questions:
- Always create exactly 4 answer options.
- All answer options must be plausible and relevant to the question.
- Randomly assign the correct answer to either the 1st, 2nd, 3rd, or 4th position (A, B, C, or D). Do not default to the first position.
- In the entire quiz, balance the distribution of correct answer positions as evenly as possible, so the correct answer appears roughly 25% of the time in each position (i.e., if there are 20 questions, about 5 in each slot).
- Do NOT put the correct answer in the first position by default.
- For the multiple-choice questions, ensure that the distribution of correct answer positions is as even as possible. Do not allow any position to have more than a quarter of the total multiple-choice questions (rounded up).

For true/false questions:
- Always use exactly two answer options: ["True", "False"], in that order. Never shuffle or reverse these.
- Ensure that, across all true/false questions, the correct answer is 'True' about half the time and 'False' about half the time.

For all questions:
- Ensure the correct answers and explanations are factually accurate and grounded in widely accepted knowledge. If the prompt is about a specific domain, use official or well-regarded sources if applicable.
- Each question should be an object with: "Question", "Options", "Answer", "Explanation", "QuestionType", and "Difficulty".
- The "QuestionType" must be exactly "TrueFalse" or "MultipleChoice" (case-sensitive).
- The "Difficulty" must be one of: "Easy", "Medium", or "Hard". Distribute difficulties roughly evenly across the quiz.
- When including C# code in questions or explanations, format it so that each statement or line of code appears on its own line, using standard C# indentation and line breaks. Do not put multiple statements on a single line.

QUIZ DIFFICULTY:
- In addition to question-level difficulty, include a "Difficulty" property at the quiz (root) level. This should represent the overall difficulty of the quiz (e.g., based on the average or predominant difficulty of the questions). Set this to one of: "Easy", "Medium", or "Hard".

OUTPUT:
- Return only valid JSON in the following format:
{{ "Questions": [ {{ "Question": "...", "Options": ["..."], "Answer": "...", "Explanation": "...", "QuestionType": "...", "Difficulty": "..." }}, ... ], "Type": "...", "Description": "...", "Difficulty": "..." }}.
- Return only the raw JSON, without any code block formatting or prefixes like 'json'.
- Do NOT include any Markdown code block formatting (such as triple backticks or the word 'json') in your response. Return only the raw JSON.

ERROR HANDLING:
- If the prompt is too vague, factually impossible, or cannot result in a meaningful quiz, return a JSON object in this format: {{ "Error": "<reason>" }}.
- If the prompt is ambiguous, choose the most likely intended topic based on the text. If still unclear, return the above error object explaining that the prompt was ambiguous.
""";
            return string.Format(prompt, url);
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
