using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WiseUpDude.Model;
using WiseUpDude.Shared.Services;

public class UserQuizApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserQuizApiService> _logger;

    public UserQuizApiService(HttpClient httpClient, ILogger<UserQuizApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Insert a new UserQuiz record using the shared Quiz model
    public async Task<Quiz?> CreateUserQuizAsync(Quiz quiz)
    {
        var response = await _httpClient.PostAsJsonAsync("api/UserQuizzes", quiz);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Quiz>();
    }

    // Update an existing UserQuiz record using the shared Quiz model
    public async Task UpdateUserQuizAsync(Quiz quiz)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserQuizzes/{quiz.Id}", quiz);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Quiz?> GetUserQuizByIdAsync(int userQuizId)
    {
        var response = await _httpClient.GetAsync($"api/UserQuizzes/{userQuizId}");
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

    // Update LearnMode for a UserQuiz
    public async Task UpdateLearnModeAsync(int userQuizId, bool learnMode)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserQuizzes/{userQuizId}/learnmode", learnMode);
        response.EnsureSuccessStatusCode();
    }

    // Update UserAnswer for a UserQuizQuestion
    public async Task UpdateUserQuizQuestionAnswerAsync(int questionId, string? userAnswer)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserQuizQuestions/{questionId}/useranswer", userAnswer);
        response.EnsureSuccessStatusCode();
    }
}