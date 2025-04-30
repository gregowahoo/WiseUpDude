using System.Text.Json;
using Microsoft.Extensions.AI;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class TopicService
    {
        private readonly IChatClient _chatClient;

        public TopicService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<(List<Topic>? Topics, string? ErrorMessage)> GetRelevantQuizTopicsAsync(List<Model.Topic> existingTopics)
        {
            try
            {
                //var prompt = "Return a JSON array of 40 objects, each with 'name' and 'description'. " +
                //             "Format: [{\"name\":\"...\",\"description\":\"...\"}, ...]. No intro or closing. " +
                //             "Return only the raw JSON without any code block formatting or prefixes like 'json'.";

                //var result = await _chatClient.GetResponseAsync(
                //    "You are a helpful assistant that suggests topics for short, engaging quizzes. " +
                //    $"Exclude the following topics: {string.Join(", ", existingTopics.Select(t => t.Name))}. " +
                //    "Can you create a list of at least 20 topics that would be interesting for people to take a quiz on? " +
                //    "Only topics that will be able to be used to create at least 20 questions from. " +
                //    "Topics should be interesting and current. Each topic should include a short 1-sentence description " +
                //    "explaining why it's interesting or relevant. \n\n" + prompt);

                var prompt = "Return a JSON array of 40 objects, each with 'name', 'description', 'category', and 'categoryDescription'. " +
                             "Format: [{\"name\":\"...\",\"description\":\"...\",\"category\":\"...\",\"categoryDescription\":\"...\"}, ...]. " +
                             "No intro or closing. Return only the raw JSON without any code block formatting or prefixes like 'json'.";

                var result = await _chatClient.GetResponseAsync(
                    "You are a helpful assistant that suggests topics for short, engaging quizzes. " +
                    $"Exclude the following topics: {string.Join(", ", existingTopics.Select(t => t.Name))}. " +
                    "Create a list of at least 20 unique topics that would be interesting for people to take a quiz on. " +
                    "Only include topics that you could generate at least 20 quiz questions for. " +
                    "Topics should be interesting and current. " +
                    "For each topic, include: " +
                    "- name: the topic name; " +
                    "- description: a short 1-sentence explanation of why the topic is interesting or relevant; " +
                    "- category: the most appropriate general knowledge category for the topic (like Science, History, Art, etc.); " +
                    "- categoryDescription: a one-sentence description of the category. " +
                    "\n\n" + prompt);

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
