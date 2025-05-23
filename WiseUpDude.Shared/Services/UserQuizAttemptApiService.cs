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

        public async Task<UserQuizAttempt?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/UserQuizAttempt/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserQuizAttempt>();
        }

        public async Task<IEnumerable<UserQuizAttempt>?> GetByUserQuizIdAsync(int userQuizId)
        {
            var response = await _httpClient.GetAsync($"api/UserQuizAttempt/byUserQuiz/{userQuizId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<IEnumerable<UserQuizAttempt>>();
        }

        public async Task<UserQuizAttempt?> CreateAsync(UserQuizAttempt attempt)
        {
            var response = await _httpClient.PostAsJsonAsync("api/UserQuizAttempt", attempt);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<UserQuizAttempt>();
        }

        public async Task UpdateAsync(UserQuizAttempt attempt)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/UserQuizAttempt/{attempt.Id}", attempt);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/UserQuizAttempt/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
