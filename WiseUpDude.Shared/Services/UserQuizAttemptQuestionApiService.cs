using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class UserQuizAttemptQuestionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserQuizAttemptQuestionApiService> _logger;

        public UserQuizAttemptQuestionApiService(HttpClient httpClient, ILogger<UserQuizAttemptQuestionApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserQuizAttemptQuestion?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/UserQuizAttemptQuestion/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserQuizAttemptQuestion>();
        }

        public async Task<IEnumerable<UserQuizAttemptQuestion>?> GetByAttemptIdAsync(int attemptId)
        {
            var response = await _httpClient.GetAsync($"api/UserQuizAttemptQuestion/byAttempt/{attemptId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<IEnumerable<UserQuizAttemptQuestion>>();
        }

        public async Task<UserQuizAttemptQuestion?> CreateAsync(UserQuizAttemptQuestion question)
        {
            var response = await _httpClient.PostAsJsonAsync("api/UserQuizAttemptQuestion", question);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserQuizAttemptQuestion>();
        }

        public async Task UpdateAsync(UserQuizAttemptQuestion question)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/UserQuizAttemptQuestion/{question.Id}", question);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/UserQuizAttemptQuestion/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
