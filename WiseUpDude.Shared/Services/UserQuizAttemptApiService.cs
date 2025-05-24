using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class UserQuizAttemptApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserQuizAttemptApiService> _logger;

        public UserQuizAttemptApiService(HttpClient httpClient, ILogger<UserQuizAttemptApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // Get a UserQuizAttempt by ID
        public async Task<UserQuizAttempt?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/UserQuizAttempts/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserQuizAttempt>();
            }
            _logger.LogWarning("Failed to get UserQuizAttempt by id {Id}. Status: {Status}", id, response.StatusCode);
            return null;
        }

        // Get all attempts for a UserQuiz
        public async Task<List<UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId)
        {
            var response = await _httpClient.GetAsync($"api/UserQuizAttempts/byUserQuiz/{userQuizId}");
            if (response.IsSuccessStatusCode)
            {
                var attempts = await response.Content.ReadFromJsonAsync<List<UserQuizAttempt>>();
                return attempts ?? new List<UserQuizAttempt>();
            }
            _logger.LogWarning("Failed to get UserQuizAttempts for UserQuizId {UserQuizId}. Status: {Status}", userQuizId, response.StatusCode);
            return new List<UserQuizAttempt>();
        }

        // Create a new UserQuizAttempt
        public async Task<UserQuizAttempt?> CreateAsync(UserQuizAttempt attempt)
        {
            var response = await _httpClient.PostAsJsonAsync("api/UserQuizAttempts", attempt);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserQuizAttempt>();
            }
            _logger.LogWarning("Failed to create UserQuizAttempt. Status: {Status}", response.StatusCode);
            return null;
        }

        // Update an existing UserQuizAttempt
        public async Task<bool> UpdateAsync(UserQuizAttempt attempt)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/UserQuizAttempts/{attempt.Id}", attempt);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            _logger.LogWarning("Failed to update UserQuizAttempt with Id {Id}. Status: {Status}", attempt.Id, response.StatusCode);
            return false;
        }

        // Delete a UserQuizAttempt
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/UserQuizAttempts/{id}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            _logger.LogWarning("Failed to delete UserQuizAttempt with Id {Id}. Status: {Status}", id, response.StatusCode);
            return false;
        }
    }
}
