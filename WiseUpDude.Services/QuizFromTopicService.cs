using Microsoft.Extensions.AI;
using System.Text.Json;
using WiseUpDude.Model;
using WiseUpDude.Services.Utilities;

namespace WiseUpDude.Services
{
    public class QuizFromTopicService
    {
        private readonly IChatClient _chatClient;

        public QuizFromTopicService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<QuizResponse?> GenerateQuizAsync(QuizRequestCriteria criteria)
        {
            //var prompt = string.Join("\n", new[]
            //{
            //    $"Create a quizResponse on the topic: \"{criteria.Topic}\".",
            //    $"The difficulty level should be: {criteria.Difficulty}.",
            //    "Use the following difficulty scale: Easy (basic knowledge), Medium (moderate understanding), Hard (advanced understanding).",
            //    "Adjust the question complexity and vocabulary based on this scale.",
            //    "Try to create at least 20 questions.",
            //    "Include both multiple-choice and true/false questions.",
            //    "For true/false questions, the options must always be: [\"True\", \"False\"].",
            //    "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\".",
            //    "When creating multiple-choice or true/false questions, randomly shuffle the answer options so the correct answer is not always first.",
            //    "The \"QuestionType\" should be either \"TrueFalse\" or \"MultipleChoice\" depending on the type of question.",
            //    "Return only valid JSON in the format:",
            //    "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"Type\": \"...\", \"Topic\": \"...\", \"Description\": \"...\" }.",
            //    "Return only the raw JSON without any code block formatting or prefixes like 'json'."
            //});

            var prompt = string.Join("\n", new[]
            {
                $"Create a quizResponse on the topic: \"{criteria.Topic}\".",
                $"The difficulty level should be: {criteria.Difficulty}.",
                "Use the following difficulty scale: Easy (basic knowledge), Medium (moderate understanding), Hard (advanced understanding).",
                "Adjust the question complexity and vocabulary based on this scale.",
                "Try to create at least 20 questions.",
                "Include both multiple-choice and true/false questions.",
                "For true/false questions, the options must always be: [\"True\", \"False\"].",
                "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\".",
                "When creating multiple-choice or true/false questions, randomly shuffle the answer options so the correct answer is not always first.",
                "For multiple-choice questions, ensure that the correct answer is evenly distributed among the answer options (A, B, C, and D). The correct answer should appear approximately the same number of times in each answer position throughout the quiz.",
                "For true/false questions, ensure that the correct answer is evenly distributed between the possible options (A and B), so that each is correct an even number of times and the correct answer does not always appear in the same option position.",
                "The \"QuestionType\" should be either \"TrueFalse\" or \"MultipleChoice\" depending on the type of question.",
                "Return only valid JSON in the format:",
                "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"Type\": \"...\", \"Topic\": \"...\", \"Description\": \"...\" }.",
                "Return only the raw JSON without any code block formatting or prefixes like 'json'."
            });

            Console.WriteLine($"Generating quizResponse for topic: {criteria.Topic} with difficulty: {criteria.Difficulty}");

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new QuizQuestionTypeConverter() }
                };

                var result = await _chatClient.GetResponseAsync(prompt);
                Console.WriteLine($"Raw AI API Response: {result.Text}");
                var json = result.Text;

                // Deserialize the response into a QuizResponse object
                var quizResponse = JsonSerializer.Deserialize<QuizResponse>(json, options) ?? new QuizResponse
                {
                    Type = "Topic",
                    Topic = criteria.Topic,
                    Description = "DefaultDescription",
                    Difficulty = criteria.Difficulty
                };

                // Ensure each question has the correct difficulty level
                foreach (var question in quizResponse.Questions)
                {
                    question.Difficulty = criteria.Difficulty; // Set question-level difficulty
                }

                // Set the quizResponse-level difficulty explicitly
                quizResponse.Difficulty = criteria.Difficulty;

                return quizResponse;
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