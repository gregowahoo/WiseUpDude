using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using WiseUpDude.Model;
using Microsoft.Extensions.Configuration;

namespace WiseUpDude.Client.Services
{
    public class QuizApiService
    {
        private readonly ILogger<QuizApiService> _logger;
        private readonly HttpClient _httpClient;

        public QuizApiService(HttpClient httpClient, ILogger<QuizApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            _logger.LogTrace("Fetching quiz with ID {QuizId}", quizId);

            var response = await _httpClient.GetAsync($"api/Quizzes/{quizId}");
            var content = await response.Content.ReadAsStringAsync(); // For debugging
            Console.WriteLine($"Status: {response.StatusCode}, Content: {content}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var quiz = await response.Content.ReadFromJsonAsync<Quiz>();
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
    }
}
