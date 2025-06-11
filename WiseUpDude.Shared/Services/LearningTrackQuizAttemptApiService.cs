using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public class LearningTrackQuizAttemptApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LearningTrackQuizAttemptApiService> _logger;

        public LearningTrackQuizAttemptApiService(HttpClient httpClient, ILogger<LearningTrackQuizAttemptApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // --- LearningTrackQuizAttempt ---
        public async Task<LearningTrackQuizAttempt?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/LearningTrackQuizAttempts/{id}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LearningTrackQuizAttempt>();
            _logger.LogWarning("Failed to get LearningTrackQuizAttempt by id {Id}. Status: {Status}", id, response.StatusCode);
            return null;
        }

        public async Task<List<LearningTrackQuizAttempt>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/LearningTrackQuizAttempts");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<LearningTrackQuizAttempt>>() ?? new();
            _logger.LogWarning("Failed to get all LearningTrackQuizAttempts. Status: {Status}", response.StatusCode);
            return new List<LearningTrackQuizAttempt>();
        }

        public async Task<LearningTrackQuizAttempt?> CreateAsync(LearningTrackQuizAttempt attempt)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LearningTrackQuizAttempts", attempt);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LearningTrackQuizAttempt>();
            _logger.LogWarning("Failed to create LearningTrackQuizAttempt. Status: {Status}", response.StatusCode);
            return null;
        }

        public async Task<bool> UpdateAsync(LearningTrackQuizAttempt attempt)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/LearningTrackQuizAttempts/{attempt.Id}", attempt);
            if (response.IsSuccessStatusCode)
                return true;
            _logger.LogWarning("Failed to update LearningTrackQuizAttempt with Id {Id}. Status: {Status}", attempt.Id, response.StatusCode);
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/LearningTrackQuizAttempts/{id}");
            if (response.IsSuccessStatusCode)
                return true;
            _logger.LogWarning("Failed to delete LearningTrackQuizAttempt with Id {Id}. Status: {Status}", id, response.StatusCode);
            return false;
        }

        // --- LearningTrackQuizAttemptQuestion ---
        public async Task<LearningTrackQuizAttemptQuestion?> GetQuestionByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/LearningTrackQuizAttempts/question/{id}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LearningTrackQuizAttemptQuestion>();
            _logger.LogWarning("Failed to get LearningTrackQuizAttemptQuestion by id {Id}. Status: {Status}", id, response.StatusCode);
            return null;
        }

        public async Task<LearningTrackQuizAttemptQuestion?> CreateQuestionAsync(LearningTrackQuizAttemptQuestion question)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LearningTrackQuizAttempts/question", question);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LearningTrackQuizAttemptQuestion>();
            _logger.LogWarning("Failed to create LearningTrackQuizAttemptQuestion. Status: {Status}", response.StatusCode);
            return null;
        }

        public async Task<bool> UpdateQuestionAsync(LearningTrackQuizAttemptQuestion question)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/LearningTrackQuizAttempts/question/{question.Id}", question);
            if (response.IsSuccessStatusCode)
                return true;
            _logger.LogWarning("Failed to update LearningTrackQuizAttemptQuestion with Id {Id}. Status: {Status}", question.Id, response.StatusCode);
            return false;
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/LearningTrackQuizAttempts/question/{id}");
            if (response.IsSuccessStatusCode)
                return true;
            _logger.LogWarning("Failed to delete LearningTrackQuizAttemptQuestion with Id {Id}. Status: {Status}", id, response.StatusCode);
            return false;
        }
    }
}
