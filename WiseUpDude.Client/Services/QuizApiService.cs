using System.Net.Http.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Client.Services
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
