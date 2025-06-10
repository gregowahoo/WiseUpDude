using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using System;
using System.Collections.Generic;

namespace WiseUpDude.Services
{
    public class PerplexityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILearningTrackQuizRepository _quizRepository;
        private readonly ILearningTrackQuizQuestionRepository _questionRepository;
        private readonly ContentFetchingService _contentFetchingService;

        public PerplexityService(
            IHttpClientFactory httpClientFactory,
            ILearningTrackQuizRepository quizRepository,
            ILearningTrackQuizQuestionRepository questionRepository,
            ContentFetchingService contentFetchingService)
        {
            _httpClientFactory = httpClientFactory;
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _contentFetchingService = contentFetchingService;
        }

        public async Task<(LearningTrackQuiz? Quiz, string? Error)> GenerateAndPersistQuizFromUrlAsync(string url, int learningTrackSourceId)
        {
            // 1. Fetch content from the URL
            var fetchResult = await _contentFetchingService.FetchValidatedTextContentAsync(url);
            if (!fetchResult.IsSuccess)
                return (null, fetchResult.ErrorMessage);

            // 2. Call Perplexity API
            var client = _httpClientFactory.CreateClient("PerplexityAI");
            var requestBody = new
            {
                model = "sonar-pro",
                messages = new[]
                {
                    new { role = "user", content = fetchResult.Content }
                }
            };

            HttpResponseMessage perplexityResponse;
            try
            {
                perplexityResponse = await client.PostAsJsonAsync("/chat/completions", requestBody);
                perplexityResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return (null, $"Perplexity API error: {ex.Message}");
            }

            string json;
            try
            {
                var perplexityContent = await perplexityResponse.Content.ReadAsStringAsync();
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
                return (null, $"Failed to parse Perplexity API response: {ex.Message}");
            }

            // 3. Parse the quiz JSON
            QuizResponse? quizResponse;
            try
            {
                quizResponse = JsonSerializer.Deserialize<QuizResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                return (null, $"Failed to parse quiz JSON: {ex.Message}");
            }

            if (quizResponse == null || quizResponse.Questions.Count == 0)
                return (null, "No quiz questions found in Perplexity response.");

            // 4. Persist to LearningTrackQuiz and LearningTrackQuizQuestions
            var quiz = new LearningTrackQuiz
            {
                Name = quizResponse.Type ?? "Perplexity Quiz",
                Description = quizResponse.Description,
                LearningTrackSourceId = learningTrackSourceId,
                CreationDate = DateTime.UtcNow,
                Questions = new List<LearningTrackQuizQuestion>()
            };
            await _quizRepository.AddQuizAsync(quiz);

            foreach (var q in quizResponse.Questions)
            {
                var question = new LearningTrackQuizQuestion
                {
                    LearningTrackQuizId = quiz.Id,
                    Question = q.Question ?? string.Empty,
                    Answer = q.Answer ?? string.Empty,
                    Explanation = q.Explanation,
                    OptionsJson = q.OptionsJson,
                    Difficulty = q.Difficulty,
                    CreationDate = DateTime.UtcNow
                };
                await _questionRepository.AddQuestionAsync(question);
                quiz.Questions.Add(question);
            }

            return (quiz, null);
        }
    }
}
