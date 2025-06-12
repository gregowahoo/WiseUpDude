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

namespace WiseUpDude.Services
{
    public class PerplexityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILearningTrackQuizRepository _quizRepository;
        private readonly ILearningTrackQuizQuestionRepository _questionRepository;
        private readonly ContentFetchingService _contentFetchingService;
        private readonly ILogger<PerplexityService> _logger;

        public PerplexityService(
            IHttpClientFactory httpClientFactory,
            ILearningTrackQuizRepository quizRepository,
            ILearningTrackQuizQuestionRepository questionRepository,
            ContentFetchingService contentFetchingService,
            ILogger<PerplexityService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _contentFetchingService = contentFetchingService;
            _logger = logger;
        }

        public async Task<(LearningTrackQuiz? Quiz, string? Error)> GenerateAndPersistQuizFromUrlAsync(string url, int learningTrackSourceId)
        {
            // 1. Build the AI prompt using the provided URL
            var aiPrompt = BuildQuizPrompt(url);

            // 2. Call Perplexity API
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
                // Try to extract the quiz JSON from the Perplexity response
                var doc = JsonDocument.Parse(perplexityContent);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content").GetString();
                json = content ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Perplexity API response");
                return (null, $"Failed to parse Perplexity API response: {ex.Message}");
            }

            // Defensive: Check if the response is valid JSON before deserializing
            if (string.IsNullOrWhiteSpace(json) || !(json.TrimStart().StartsWith("{") || json.TrimStart().StartsWith("[")))
            {
                _logger.LogWarning("Perplexity did not return valid JSON. Raw response: {Raw}", json);
                return (null, $"Perplexity did not return valid JSON. Raw response: {json}");
            }

            // 3. Parse the quiz JSON
            Quiz? quizModel;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter());
                quizModel = JsonSerializer.Deserialize<Quiz>(json, options);
            }
            catch (Exception ex)
            {
                return (null, $"Failed to parse quiz JSON: {ex.Message}. Raw response: {json}");
            }

            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Perplexity response.");

            // 4. Persist to LearningTrackQuiz and LearningTrackQuizQuestions
            var quiz = new LearningTrackQuiz
            {
                Name = quizModel.Type ?? "Perplexity Quiz",
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

            return (quiz, null);
        }

        private string BuildQuizPrompt(string url)
        {
            var prompt = """
Create a quiz based on the following prompt: "Use this URL: {0}"
The quiz should include at least 20 questions.
Include both multiple-choice and true/false questions.

QUESTION FORMATTING & ANSWER SHUFFLING:
For multiple-choice questions:
- Always create exactly 4 answer options.
- All answer options must be plausible and relevant to the question.
- Randomly assign the correct answer to either the 1st, 2nd, 3rd, or 4th position (A, B, C, or D). Do not default to the first position.
- In the entire quiz, balance the distribution of correct answer positions as evenly as possible, so the correct answer appears roughly 25% of the time in each position (i.e., if there are 20 questions, about 5 in each slot).
- Do NOT put the correct answer in the first position by default.
- For the 20 multiple-choice questions, ensure that exactly 5 questions have the correct answer in position 1, 5 in position 2, 5 in position 3, and 5 in position 4. Track and enforce this distribution as you generate the quiz. Do not allow any position to have more than 5 correct answers.

For true/false questions:
- Always use exactly two answer options: ["True", "False"], in that order. Never shuffle or reverse these.
- Ensure that, across all true/false questions, the correct answer is 'True' about half the time and 'False' about half the time.

For all questions:
- Ensure the correct answers and explanations are factually accurate and grounded in widely accepted knowledge. If the prompt is about a specific domain, use official or well-regarded sources if applicable.
- Each question should be an object with: "Question", "Options", "Answer", "Explanation", and "QuestionType".
- The "QuestionType" must be exactly "TrueFalse" or "MultipleChoice" (case-sensitive).

OUTPUT:
- Return only valid JSON in the following format:
{{ "Questions": [ {{ "Question": "...", "Options": ["..."], "Answer": "...", "Explanation": "...", "QuestionType": "..." }}, ... ], "Type": "...", "Description": "..." }}.
- Return only the raw JSON, without any code block formatting or prefixes like 'json'.

ERROR HANDLING:
- If the prompt is too vague, factually impossible, or cannot result in a meaningful quiz, return a JSON object in this format: {{ "Error": "<reason>" }}.
- If the prompt is ambiguous, choose the most likely intended topic based on the text. If still unclear, return the above error object explaining that the prompt was ambiguous.
""";
            return string.Format(prompt, url);
        }
    }
}
