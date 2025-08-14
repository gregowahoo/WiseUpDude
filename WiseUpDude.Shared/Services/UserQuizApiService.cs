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
            _logger.LogInformation("UserQuizApi: Creating UserQuiz via API. Base={Base} Path={Path}", _httpClient.BaseAddress, "api/UserQuizzes");
            var response = await _httpClient.PostAsJsonAsync("api/UserQuizzes", quiz);
            _logger.LogInformation("UserQuizApi: Create response {Status}", response.StatusCode);
            response.EnsureSuccessStatusCode();
            var createdQuiz = await response.Content.ReadFromJsonAsync<Quiz>();
            if (createdQuiz == null || createdQuiz.Id == 0)
            {
                _logger.LogError("UserQuizApi: Create returned null or invalid Id. Payload: {@Quiz}", quiz);
            }
            else
            {
                _logger.LogInformation("UserQuizApi: Created UserQuiz with Id={QuizId}", createdQuiz.Id);
            }
            return createdQuiz;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserQuizApi: Exception creating UserQuiz. Payload: {@Quiz}", quiz);
            return null;
        }
    }

    // Update an existing UserQuiz record using the shared Quiz model
    public async Task UpdateUserQuizAsync(Quiz quiz)
    {
        _logger.LogInformation("UserQuizApi: Updating quiz id={Id}", quiz.Id);
        var response = await _httpClient.PutAsJsonAsync($"api/UserQuizzes/{quiz.Id}", quiz);
        _logger.LogInformation("UserQuizApi: Update response {Status}", response.StatusCode);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Quiz?> GetUserQuizByIdAsync(int userQuizId)
    {
        var path = $"api/UserQuizzes/{userQuizId}";
        _logger.LogInformation("UserQuizApi: GET {Path} Base={Base}", path, _httpClient.BaseAddress);
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("UserQuizApi: GET status={Status} length={Length}", response.StatusCode, content?.Length ?? 0);

            if (response.IsSuccessStatusCode)
            {
                var quiz = await response.Content.ReadFromJsonAsync<Quiz>();
                if (quiz == null)
                {
                    _logger.LogWarning("UserQuizApi: Deserialization returned null for path {Path}", path);
                }
                return quiz;
            }
            else
            {
                _logger.LogWarning("UserQuizApi: Non-success status {Status} for path {Path}. Body={Body}", response.StatusCode, path, content);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserQuizApi: Exception GET {Path}", path);
        }
        return null;
    }

    // Update LearnMode for a UserQuiz
    public async Task UpdateLearnModeAsync(int userQuizId, bool learnMode)
    {
        var path = $"api/UserQuizzes/{userQuizId}/learnmode";
        _logger.LogInformation("UserQuizApi: PUT {Path} learnMode={LearnMode}", path, learnMode);
        var response = await _httpClient.PutAsJsonAsync(path, learnMode);
        _logger.LogInformation("UserQuizApi: LearnMode response {Status}", response.StatusCode);
        response.EnsureSuccessStatusCode();
    }

    // Update UserAnswer for a UserQuizQuestion
    public async Task UpdateUserQuizQuestionAnswerAsync(int questionId, string? userAnswer)
    {
        var path = $"api/UserQuizQuestions/{questionId}/useranswer";
        _logger.LogInformation("UserQuizApi: PUT {Path}", path);
        var response = await _httpClient.PutAsJsonAsync(path, userAnswer);
        _logger.LogInformation("UserQuizApi: UpdateUserAnswer response {Status}", response.StatusCode);
        response.EnsureSuccessStatusCode();
    }

    // Copies a quiz by id and creates a special assignment in one operation
    public async Task<(bool Success, int? NewQuizId)> CreateSpecialAssignmentWithQuizCopyAsync(int quizId, SpecialQuizAssignment assignment)
    {
        try
        {
            _logger.LogInformation("UserQuizApi: Copy quiz id {QuizId} via API.", quizId);
            var copyResponse = await _httpClient.PostAsync($"api/UserQuizzes/{quizId}/copy", null);
            _logger.LogInformation("UserQuizApi: Copy response {Status}", copyResponse.StatusCode);
            if (!copyResponse.IsSuccessStatusCode)
            {
                _logger.LogError("UserQuizApi: Failed to copy quiz. Status: {Status}", copyResponse.StatusCode);
                return (false, null);
            }

            var newQuizId = await copyResponse.Content.ReadFromJsonAsync<int>();
            if (newQuizId == 0)
            {
                _logger.LogError("UserQuizApi: Copy API returned invalid new QuizId.");
                return (false, null);
            }

            assignment.UserQuizId = newQuizId;
            var assignResponse = await _httpClient.PostAsJsonAsync("/api/SpecialQuizAssignments", assignment);
            _logger.LogInformation("UserQuizApi: Special assignment response {Status}", assignResponse.StatusCode);
            if (!assignResponse.IsSuccessStatusCode)
            {
                _logger.LogError("UserQuizApi: Failed to submit SpecialQuizAssignment. Status: {Status}, Payload: {@Assignment}", assignResponse.StatusCode, assignment);
                return (false, newQuizId);
            }

            _logger.LogInformation("UserQuizApi: Special assignment created for new quiz {QuizId}", newQuizId);
            return (true, newQuizId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserQuizApi: Exception in CreateSpecialAssignmentWithQuizCopyAsync for QuizId={QuizId}", quizId);
            return (false, null);
        }
    }

    public async Task<List<Quiz>> GetAllAsync()
    {
        _logger.LogInformation("UserQuizApi: GET api/UserQuizzes Base={Base}", _httpClient.BaseAddress);
        return await _httpClient.GetFromJsonAsync<List<Quiz>>("api/UserQuizzes");
    }
}