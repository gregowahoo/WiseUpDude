// QuizTopicService_OpenAI.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace WiseUpDude.Services
{
    public class QuizTopicService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public QuizTopicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
        }

        public async Task<(List<(string Topic, string Description)>? Topics, string? ErrorMessage)> GetRelevantQuizTopicsAsync()
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
                        new { role = "system", content = "You are a helpful assistant that suggests topics for short, engaging quizzes. Each topic should include a short 1-sentence description explaining why it's interesting or relevant." },
                        new { role = "user", content = "Return a JSON array of 20 objects, each with 'topic' and 'description'. Format: [{\"topic\":\"...\",\"description\":\"...\"}, ...]. No intro or closing." }
                    },
                    temperature = 0.7,
                    max_tokens = 600
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
                        try
                        {
                            string jsonArray = ExtractJsonArray(content);
                            //var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var topics = JsonSerializer.Deserialize<List<QuizTopicService.TopicItem>>(jsonArray);

                            if (topics != null)
                            {
                                var result = topics.Select(t => (t.Topic, t.Description)).ToList();
                                return (result, null);
                            }

                            return (null, "Failed to parse topic data from OpenAI.");
                        }
                        catch (JsonException jex)
                        {
                            return (null, $"JSON parsing error: {jex.Message}");
                        }
                    }

                    return (null, "No topics were returned from the OpenAI API.");
                }

                return (null, "Invalid response format from OpenAI API.");
            }
            catch (Exception ex)
            {
                return (null, $"Error: {ex.Message}");
            }
        }

        private static string ExtractJsonArray(string content)
        {
            var start = content.IndexOf('[');
            var end = content.LastIndexOf(']');

            if (start >= 0 && end > start)
            {
                return content.Substring(start, end - start + 1);
            }

            return "[]";
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

        public class TopicItem
        {
            [JsonPropertyName("topic")]
            public string Topic { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }

    }
}
