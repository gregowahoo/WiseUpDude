using System.Net.Http.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class QuizApiService
    {
        private readonly HttpClient _httpClient;

        public QuizApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            // Adjust the API endpoint as needed
            var response = await _httpClient.GetAsync($"api/Quizzes/{quizId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Quiz>();
            }
            return null;
        }
    }
}
