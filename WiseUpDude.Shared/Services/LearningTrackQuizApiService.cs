using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class LearningTrackQuizApiService
    {
        private readonly ILogger<LearningTrackQuizApiService> _logger;
        private readonly HttpClient _httpClient;

        public LearningTrackQuizApiService(HttpClient httpClient, ILogger<LearningTrackQuizApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<LearningTrackQuiz?> GetQuizByIdAsync(int quizId)
        {
            _logger.LogTrace("Fetching learning track quiz with ID {QuizId}", quizId);

            var response = await _httpClient.GetAsync($"api/LearningTrackQuizzes/{quizId}");
            var content = await response.Content.ReadAsStringAsync(); // For debugging
            Console.WriteLine($"Status: {response.StatusCode}, Content: {content}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var quiz = await response.Content.ReadFromJsonAsync<LearningTrackQuiz>();
                    if (quiz == null)
                    {
                        Console.WriteLine("Deserialization returned null.");
                    }
                    return quiz;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public async Task<IEnumerable<LearningTrackQuiz>?> GetAllQuizzesAsync()
        {
            _logger.LogTrace("Fetching all learning track quizzes");

            var response = await _httpClient.GetAsync("api/LearningTrackQuizzes");
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<LearningTrackQuiz>>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        public async Task<bool> CreateQuizAsync(LearningTrackQuiz quiz)
        {
            _logger.LogTrace("Creating new learning track quiz");
            var response = await _httpClient.PostAsJsonAsync("api/LearningTrackQuizzes", quiz);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateQuizAsync(LearningTrackQuiz quiz)
        {
            _logger.LogTrace("Updating learning track quiz with ID {QuizId}", quiz.Id);
            var response = await _httpClient.PutAsJsonAsync($"api/LearningTrackQuizzes/{quiz.Id}", quiz);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            _logger.LogTrace("Deleting learning track quiz with ID {QuizId}", quizId);
            var response = await _httpClient.DeleteAsync($"api/LearningTrackQuizzes/{quizId}");
            return response.IsSuccessStatusCode;
        }
    }
}