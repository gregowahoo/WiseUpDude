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
        try
        {
            _logger.LogInformation("Attempting to create a new UserQuiz via API.");
            var response = await _httpClient.PostAsJsonAsync("api/UserQuizzes", quiz);
            response.EnsureSuccessStatusCode();
            var createdQuiz = await response.Content.ReadFromJsonAsync<Quiz>();
            if (createdQuiz == null || createdQuiz.Id == 0)
            {
                _logger.LogError("Quiz creation API returned null or invalid Id. Payload: {@Quiz}", quiz);
            }
            else
            {
                _logger.LogInformation("Successfully created UserQuiz with Id={QuizId}", createdQuiz.Id);
            }
            return createdQuiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating UserQuiz. Payload: {@Quiz}", quiz);
            return null;
        }
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

    // Copies a quiz by id and creates a special assignment in one operation
    public async Task<(bool Success, int? NewQuizId)> CreateSpecialAssignmentWithQuizCopyAsync(int quizId, SpecialQuizAssignment assignment)
    {
        try
        {
            _logger.LogInformation("Copying quiz with id {QuizId} via API.", quizId);
            var copyResponse = await _httpClient.PostAsync($"api/UserQuizzes/{quizId}/copy", null);
            if (!copyResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to copy quiz. Status: {Status}", copyResponse.StatusCode);
                return (false, null);
            }

            var newQuizId = await copyResponse.Content.ReadFromJsonAsync<int>();
            if (newQuizId == 0)
            {
                _logger.LogError("Copy API returned invalid new QuizId.");
                return (false, null);
            }

            assignment.UserQuizId = newQuizId;
            var assignResponse = await _httpClient.PostAsJsonAsync("/api/SpecialQuizAssignments", assignment);
            if (!assignResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to submit SpecialQuizAssignment. Status: {Status}, Payload: {@Assignment}", assignResponse.StatusCode, assignment);
                return (false, newQuizId);
            }

            _logger.LogInformation("Special assignment created for new quiz {QuizId}", newQuizId);
            return (true, newQuizId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in CreateSpecialAssignmentWithQuizCopyAsync for QuizId={QuizId}", quizId);
            return (false, null);
        }
    }
}