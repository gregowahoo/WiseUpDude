using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WiseUpDude.Model;
using WiseUpDude.Services.Utilities;

namespace WiseUpDude.Services
{
    public class QuizFromTopicService
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<QuizFromTopicService> _logger;
        public QuizFromTopicService(IChatClient chatClient, ILogger<QuizFromTopicService> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<QuizResponse?> GenerateQuizAsync(QuizRequestCriteria criteria)
        {
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

            _logger.LogInformation("Starting quiz generation for topic: {Topic}, difficulty: {Difficulty}", criteria.Topic, criteria.Difficulty);
            _logger.LogDebug("Prompt sent to AI API: {Prompt}", prompt);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new QuizQuestionTypeConverter() }
                };

                _logger.LogInformation("Calling AI API...");
                var result = await _chatClient.GetResponseAsync(prompt);

                if (result == null)
                {
                    _logger.LogError("AI API returned null result for topic: {Topic}", criteria.Topic);
                    return null;
                }

                _logger.LogInformation("AI API call succeeded. Raw response length: {Length}", result.Text?.Length ?? 0);
                _logger.LogDebug("Raw AI API Response: {Response}", result.Text);

                var json = result.Text;

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogError("AI API returned empty response for topic: {Topic}", criteria.Topic);
                    return null;
                }

                QuizResponse? quizResponse = null;
                try
                {
                    quizResponse = JsonSerializer.Deserialize<QuizResponse>(json, options);
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "JSON deserialization failed for topic: {Topic}. Raw response: {Response}", criteria.Topic, json);
                    return null;
                }

                if (quizResponse == null)
                {
                    _logger.LogError("Deserialized QuizResponse is null for topic: {Topic}. Raw response: {Response}", criteria.Topic, json);
                    return null;
                }

                // Ensure each question has the correct difficulty level
                if (quizResponse.Questions != null)
                {
                    foreach (var question in quizResponse.Questions)
                    {
                        question.Difficulty = criteria.Difficulty;
                    }
                }
                else
                {
                    _logger.LogWarning("QuizResponse.Questions is null for topic: {Topic}", criteria.Topic);
                }

                quizResponse.Difficulty = criteria.Difficulty;

                _logger.LogInformation("Quiz generation completed successfully for topic: {Topic}", criteria.Topic);
                return quizResponse;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Quiz generation failed for topic: {Topic}, difficulty: {Difficulty}", criteria.Topic, criteria.Difficulty);
                _logger.LogError(ex, "Quiz generation failed for topic: {Topic}, difficulty: {Difficulty}. Exception: {Exception}", criteria.Topic, criteria.Difficulty, ex.ToString());
                return null;
            }
        }
    }
}