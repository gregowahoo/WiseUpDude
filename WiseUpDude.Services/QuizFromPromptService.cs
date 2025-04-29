using Microsoft.Extensions.AI;
using System.Text.Json;
using WiseUpDude.Model;
using WiseUpDude.Services.Interfaces;

namespace WiseUpDude.Services
{

    public class QuizFromPromptService : IQuizGenerationService
    {
        private readonly IChatClient _chatClient;

        public QuizFromPromptService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<QuizResponse?> GenerateQuizFromPromptAsync(string prompt)
        {
            var aiPrompt = string.Join("\n", new[]
            {
                $"Create a quiz based on the following prompt: \"{prompt}\".",
                "The quiz should include at least 20 questions.",
                "Include both multiple-choice and true/false questions.",
                "For true/false questions, the options must always be: [\"True\", \"False\"].",
                "For multiple-choice questions, there must be exactly 4 answer options.",
                "Ensure that all answer options for multiple-choice questions are plausible and relevant to the question.",
                "Ensure that the correct answers and explanations are factually accurate based on Blazor Server's official documentation.",
                "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\".",
                "The \"QuestionType\" must be exactly \"TrueFalse\" or \"MultipleChoice\" (case-sensitive).",
                "Return only valid JSON in the format:",
                "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"Type\": \"...\", \"Description\": \"...\" }.",
                "Return only the raw JSON without any code block formatting or prefixes like 'json'."
            });

            Console.WriteLine($"Generating quiz from user prompt: {prompt}");

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new QuizQuestionTypeConverter() } // Added the custom converter
                };

                // Get the raw AI response
                var result = await _chatClient.GetResponseAsync(aiPrompt);
                var json = result.Text;
                Console.WriteLine($"Raw AI API Response: {json}");

                // Step 1: Deserialize into a temporary object for validation
                QuizResponse? parsedJson;
                try
                {
                    parsedJson = JsonSerializer.Deserialize<QuizResponse>(json, options);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parsing error during validation: {ex.Message}");
                    return null;
                }

                // Step 2: Confirm that parsedJson is not null and that every QuestionType is valid
                if (parsedJson?.Questions == null || !parsedJson.Questions.All(q =>
                        q.QuestionType == QuizQuestionType.TrueFalse ||
                        q.QuestionType == QuizQuestionType.MultipleChoice))
                {
                    Console.WriteLine("Invalid QuestionType values in AI API response.");
                    return null;
                }

                // Step 3: At this point, the temporary object is valid. We can reuse parsedJson 
                // or deserialize again, but we'll reuse it to avoid a second parse.
                var quizResponse = parsedJson;

                // Step 4: Verify the response contains valid quiz data
                if (!quizResponse.Questions.Any())
                {
                    Console.WriteLine("AI API response does not contain valid quiz data.");
                    return null;
                }

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
