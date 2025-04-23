using Microsoft.Extensions.AI;
using System.Text.Json;
using WiseUpDude.Model;
using WiseUpDude.Services.Utilities;

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
                "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\".",
                "The \"QuestionType\" should be either \"TrueFalse\" or \"MultipleChoice\" depending on the type of question.",
                "Additionally, include a \"QuizSource\" object with the following properties:",
                "{ \"Type\": \"Name\", \"Name\": \"<topic>\", \"Description\": \"<description>\" }.",
                "Return only valid JSON in the format:",
                "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"QuizSource\": { \"Type\": \"...\", \"Name\": \"...\", \"Description\": \"...\" } }.",
                "Return only the raw JSON without any code block formatting or prefixes like 'json'."
            });

            // Add logging for debugging
            Console.WriteLine($"Generating quiz for topic: {criteria.Topic} with difficulty: {criteria.Difficulty}");

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new QuizQuestionTypeConverter() }
                };

                var result = await _chatClient.GetResponseAsync(prompt);                
                Console.WriteLine($"Raw AI API Response: {result.Text}");           // Log the raw response for debugging
                var json = result.Text;
                var quiz = JsonSerializer.Deserialize<QuizResponse>(json, options) ?? new QuizResponse
                {
                    QuizSource = new QuizSource
                    {
                        Type = "DefaultType",
                        Topic = "DefaultTopic",
                        Description = "DefaultDescription"
                    }
                };

                return quiz;
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"Quiz_Orig JSON parse error: {jex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Quiz_Orig generation failed: {ex.Message}");
                return null;
            }
        }
    }
}