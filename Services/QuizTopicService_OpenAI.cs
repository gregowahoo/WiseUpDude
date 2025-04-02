// QuizTopicService_OpenAI.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace WiseUpDude.Services
{
    public class QuizTopicService_OpenAI
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public QuizTopicService_OpenAI(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
        }

        public async Task<(List<string>? Topics, string? ErrorMessage)> GetRelevantQuizTopicsAsync()
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return (null, "OpenAI API key is not configured.");
            }

            try
            {
                var requestBody = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a helpful assistant that suggests topics for short, engaging quizzes." },
                    new { role = "user", content = "Suggest 20 current and relevant topics that people would be interested in taking a short quiz about. Give me only the topic names, not any extra information." }
                },
                    temperature = 0.7,
                    max_tokens = 300
                };

                var jsonRequestBody = JsonSerializer.Serialize(requestBody);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
                {
                    Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (null, $"OpenAI API error: {response.StatusCode} - {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

                if (responseObject?.choices != null && responseObject.choices.Length > 0)
                {
                    string? content = responseObject.choices[0]?.message?.content;
                    if (!string.IsNullOrEmpty(content))
                    {
                        var topics = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(t => t.Trim().TrimStart('-').Trim())
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

        public class OpenAIResponse
        {
            public Choice[]? choices { get; set; }
        }

        public class Choice
        {
            public Message? message { get; set; }
        }

        public class Message
        {
            public string? role { get; set; }
            public string? content { get; set; }
        }
    }
}