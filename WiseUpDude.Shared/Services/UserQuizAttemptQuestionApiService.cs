using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class UserQuizAttemptQuestionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserQuizAttemptQuestionApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public UserQuizAttemptQuestionApiService(HttpClient httpClient, ILogger<UserQuizAttemptQuestionApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserQuizAttemptQuestion?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching UserQuizAttemptQuestion with ID: {Id}", id);
                var response = await _httpClient.GetAsync($"api/UserQuizAttemptQuestion/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error retrieving UserQuizAttemptQuestion. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return null;
                }
                
                return await response.Content.ReadFromJsonAsync<UserQuizAttemptQuestion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching UserQuizAttemptQuestion: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<UserQuizAttemptQuestion>?> GetByAttemptIdAsync(int attemptId)
        {
            try
            {
                _logger.LogInformation("Fetching UserQuizAttemptQuestions for attempt ID: {AttemptId}", attemptId);
                var response = await _httpClient.GetAsync($"api/UserQuizAttemptQuestion/byAttempt/{attemptId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error retrieving UserQuizAttemptQuestions. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return null;
                }
                
                return await response.Content.ReadFromJsonAsync<IEnumerable<UserQuizAttemptQuestion>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching UserQuizAttemptQuestions: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<UserQuizAttemptQuestion?> CreateAsync(UserQuizAttemptQuestion question)
        {
            try
            {
                _logger.LogInformation("Creating UserQuizAttemptQuestion: AttemptId={AttemptId}, QuestionId={QuestionId}", 
                    question.UserQuizAttemptId, question.UserQuizQuestionId);
                
                var jsonContent = JsonSerializer.Serialize(question, _jsonOptions);
                _logger.LogDebug("Request payload: {JsonContent}", jsonContent);
                
                var response = await _httpClient.PostAsJsonAsync("api/UserQuizAttemptQuestion", question);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error creating UserQuizAttemptQuestion. StatusCode: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    return null;
                }
                
                _logger.LogInformation("Successfully created UserQuizAttemptQuestion");
                return await response.Content.ReadFromJsonAsync<UserQuizAttemptQuestion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while creating UserQuizAttemptQuestion: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<UserQuizAttemptQuestion?> CreateOrUpdateAsync(UserQuizAttemptQuestion question)
        {
            try
            {
                _logger.LogInformation("Creating or updating UserQuizAttemptQuestion: AttemptId={AttemptId}, QuestionId={QuestionId}, Answer={Answer}", 
                    question.UserQuizAttemptId, question.UserQuizQuestionId, question.UserAnswer);
                
                var jsonContent = JsonSerializer.Serialize(question, _jsonOptions);
                _logger.LogDebug("Request payload: {JsonContent}", jsonContent);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/UserQuizAttemptQuestion/CreateOrUpdate", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error creating/updating UserQuizAttemptQuestion. StatusCode: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    return null;
                }
                
                _logger.LogInformation("Successfully created/updated UserQuizAttemptQuestion. Response: {Response}", responseContent);
                return JsonSerializer.Deserialize<UserQuizAttemptQuestion>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while creating/updating UserQuizAttemptQuestion: {Message}", ex.Message);
                return null;
            }
        }

        public async Task UpdateAsync(UserQuizAttemptQuestion question)
        {
            try
            {
                _logger.LogInformation("Updating UserQuizAttemptQuestion with ID: {Id}", question.Id);
                var jsonContent = JsonSerializer.Serialize(question, _jsonOptions);
                _logger.LogDebug("Request payload: {JsonContent}", jsonContent);
                
                var response = await _httpClient.PutAsJsonAsync($"api/UserQuizAttemptQuestion/{question.Id}", question);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error updating UserQuizAttemptQuestion. StatusCode: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    response.EnsureSuccessStatusCode(); // This will throw an exception
                }
                
                _logger.LogInformation("Successfully updated UserQuizAttemptQuestion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating UserQuizAttemptQuestion: {Message}", ex.Message);
                throw; // Rethrow to maintain the same behavior
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting UserQuizAttemptQuestion with ID: {Id}", id);
                var response = await _httpClient.DeleteAsync($"api/UserQuizAttemptQuestion/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error deleting UserQuizAttemptQuestion. StatusCode: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    response.EnsureSuccessStatusCode(); // This will throw an exception
                }
                
                _logger.LogInformation("Successfully deleted UserQuizAttemptQuestion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while deleting UserQuizAttemptQuestion: {Message}", ex.Message);
                throw; // Rethrow to maintain the same behavior
            }
        }
    }
}
