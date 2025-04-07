using System.Text.Json;
using Microsoft.Extensions.AI;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class QuizQuestionsFromTopic
    {
        private readonly IChatClient _chatClient;

        public QuizQuestionsFromTopic(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<QuizResponse?> GenerateQuizAsync(QuizRequestCriteria criteria)
        {
            var prompt = string.Join("\n", new[]
            {
                $"Create a quiz on the topic: \"{criteria.Topic}\".",
                $"The difficulty level should be: {criteria.Difficulty}.",
                "Use the following difficulty scale: Easy (basic knowledge), Medium (moderate understanding), Hard (advanced understanding).",
                "Adjust the question complexity and vocabulary based on this scale.",
                "Try to create at least 20 questions.",
                "Include both multiple-choice and true/false questions.",
                "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\".",
                "Return only valid JSON in the format:",
                "{ \"Questions\": [ { ... }, ... ] }.",
                "Return only the raw JSON without any code block formatting or prefixes like 'json'."
            });

            try
            {
                var result = await _chatClient.GetResponseAsync(prompt);
                var json = result.Text;
                var quiz = JsonSerializer.Deserialize<QuizResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return quiz;
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"Quiz JSON parse error: {jex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Quiz generation failed: {ex.Message}");
                return null;
            }
        }
    }
}