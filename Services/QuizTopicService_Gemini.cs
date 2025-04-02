using System.Net.Http.Headers;
using System.Text;

using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace WiseUpDude.Services
{
    public class QuizTopicService_Gemini
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? _openAIKey;

        public QuizTopicService_Gemini(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _openAIKey = _configuration["OpenAI:ApiKey"]; // Load the API key
        }

        public async Task<(List<string>? Topics, string? ErrorMessage)> GetRelevantQuizTopicsAsync()
        {
            if (string.IsNullOrEmpty(_openAIKey))
            {
                return (null, "OpenAI API key is not configured.");
            }

            try
            {
                // Use the Chat Completions API
                var apiUrl = "https://api.openai.com/v1/chat/completions";
                var modelName = "gpt-3.5-turbo"; // Or "gpt-4"

                // System prompt to guide the model's behavior
                var systemPrompt = new
                {
                    role = "system",
                    content = "You are a helpful assistant that suggests topics for short, engaging quizzes."
                };

                // User prompt to request quiz topics
                var userPrompt = new
                {
                    role = "user",
                    content = "Suggest 20 current and relevant topics that people would be interested in taking a short quiz about. Topics should be interesting and current."
                };

                // Create the request payload
                var requestBody = new
                {
                    model = modelName,
                    messages = new[] { systemPrompt, userPrompt }, // Use an array of messages
                    temperature = 0.7, // Adjust for more/less randomness
                    max_tokens = 100 // Limit the response length
                };

                // Serialize the request body to JSON
                var jsonRequestBody = JsonSerializer.Serialize(requestBody);

                // Create the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAIKey);
                request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                // Send the request and get the response
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (null, $"OpenAI API error: {response.StatusCode} - {errorContent}");
                }

                // Deserialize the response
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

                if (responseObject?.choices != null && responseObject.choices.Length > 0)
                {
                    // Extract the content from the first choice.
                    string? content = responseObject.choices[0]?.message?.content;

                    if (content != null)
                    {
                        // Split the content into individual topics, removing leading/trailing whitespace.
                        var topics = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim().TrimStart('-').Trim()) // Remove extra characters
                                        .ToList();
                        return (topics, null);
                    }
                    else
                    {
                        return (null, "No topics were returned from the OpenAI API.");
                    }
                }
                else
                {
                    return (null, "Invalid response format from OpenAI API.");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Error: {ex.Message}");
            }
        }

        // Define the structure of the OpenAI response, using the Chat Completions format.
        public class OpenAIResponse
        {
            public Choice[]? choices { get; set; }
        }

        public class Choice
        {
            public Message? message { get; set; }
            public string? finish_reason { get; set; }
            public int index { get; set; }
        }

        public class Message
        {
            public string? role { get; set; }
            public string? content { get; set; }
        }
    }
}