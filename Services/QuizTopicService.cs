using System.Text.Json;
using Microsoft.Extensions.AI;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class QuizTopicService
    {
        private readonly IChatClient _chatClient;

        public QuizTopicService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<(List<Topic>? Topics, string? ErrorMessage)> GetRelevantQuizTopicsAsync()
        {
            try
            {
                var prompt = "Return a JSON array of 40 objects, each with 'topic' and 'description'. " +
                             "Format: [{\"name\":\"...\",\"description\":\"...\"}, ...]. No intro or closing. " +
                             "Return only the raw JSON without any code block formatting or prefixes like 'json'.";

                var result = await _chatClient.GetResponseAsync(
                    "You are a helpful assistant that suggests topics for short, engaging quizzes. " +
                    "Can you create a list of at least 20 topics that would be interesting for people to take a quiz on? " +
                    "Only topics that will be able to be used to create at least 20 questions from. " +
                    "Topics should be interesting and current. Each topic should include a short 1-sentence description " +
                    "explaining why it's interesting or relevant. \n\n" + prompt);
                var content = result.Text;

                try
                {
                    string jsonArray = ExtractJsonArray(content);
                    var topics = JsonSerializer.Deserialize<List<Topic>>(jsonArray);
                    return topics != null
                        ? (topics, null)
                        : (null, "Failed to parse topic data from model response.");
                }
                catch (JsonException jex)
                {
                    return (null, $"JSON parsing error: {jex.Message}");
                }
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
            return (start >= 0 && end > start) ? content.Substring(start, end - start + 1) : "[]";
        }
    }
}
