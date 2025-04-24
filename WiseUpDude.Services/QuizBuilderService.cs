using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using Microsoft.Extensions.AI;
using WiseUpDude.Model;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WiseUpDude.Services
{
    public class QuizBuilderService
    {
        private readonly IChatClient _chatClient;

        public QuizBuilderService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<QuizResponse> GenerateQuizAsync(string content, int maxQuestions)
        {
            var quizResponse = new QuizResponse
            {
                Type = "Generated",
                Topic = "AI-Generated Quiz",
                Description = "This quiz was generated based on the provided content."
            };
            var allQuestions = new List<QuizQuestion>();

            try
            {
                int chunkSize = 4000;
                var chunks = SplitContent(content, chunkSize);
                bool isFirstChunk = true;

                foreach (var chunk in chunks)
                {
                    var prompt = isFirstChunk
                        ? $"Generate a quiz based on the following content:\n\n{chunk}\n\n" +
                          "The difficulty level should be assigned to each question based on its complexity. Use the following scale: Easy (basic knowledge), Medium (moderate understanding), Hard (advanced understanding)." +
                          "Include a balanced mix of multiple-choice and true/false questions. Aim for approximately 50% multiple-choice and 50% true/false questions." +
                          "For true/false questions, the options must always be: [\"True\", \"False\"]." +
                          "Ensure that the 'Answer' and 'Explanation' fields are logically consistent. The 'Explanation' must justify the 'Answer'." +
                          "Generate no more than 25 questions in total." +
                          "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\", \"Difficulty\"." + // Added Difficulty field
                          "The \"QuestionType\" should be either \"TrueFalse\" or \"MultipleChoice\" depending on the type of question." +
                          "Return only valid JSON in the format:" +
                          "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\", \"Difficulty\": \"...\" }, ... ], \"Type\": \"...\", \"Topic\": \"...\", \"Description\": \"...\" }." +
                          "Return only the raw JSON without any code block formatting or prefixes like 'json'."
                        : $"Continue generating quiz questions based on the following content:\n\n{chunk}\n\n" +
                          "Maintain the style, format, and JSON structure from the previous questions." +
                          "The difficulty level should be assigned to each question based on its complexity. Use the following scale: Easy (basic knowledge), Medium (moderate understanding), Hard (advanced understanding)." +
                          "Include a balanced mix of multiple-choice and true/false questions. Aim for approximately 50% multiple-choice and 50% true/false questions." +
                          "For true/false questions, the options must always be: [\"True\", \"False\"]." +
                          "Ensure that the 'Answer' and 'Explanation' fields are logically consistent. The 'Explanation' must justify the 'Answer'." +
                          "Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\", \"Difficulty\"." + // Added Difficulty field
                          "The \"QuestionType\" should be either \"TrueFalse\" or \"MultipleChoice\" depending on the type of question." +
                          "Return only valid JSON in the format:" +
                          "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\", \"Difficulty\": \"...\" }, ... ], \"Type\": \"...\", \"Topic\": \"...\", \"Description\": \"...\" }." +
                          "Return only the raw JSON without any code block formatting or prefixes like 'json'.";

                    isFirstChunk = false;

                    var result = await _chatClient.GetResponseAsync(prompt);
                    var quizJson = result.Text;

                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new QuizQuestionTypeConverter() }
                        };

                        // Validate JSON string
                        using (JsonDocument doc = JsonDocument.Parse(quizJson))
                        {
                            var chunkQuizResponse = JsonSerializer.Deserialize<QuizResponse>(quizJson, options);
                            if (chunkQuizResponse?.Questions != null)
                            {
                                allQuestions.AddRange(chunkQuizResponse.Questions);
                                
                                if (allQuestions.Count >= maxQuestions)     // Check if the total number of questions exceeds the maxQuestions limit
                                {
                                    quizResponse.Questions = allQuestions.Take(maxQuestions).ToList(); // Limit to maxQuestions
                                    quizResponse.Difficulty = "Mixed";
                                    return quizResponse;
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Parsing Error: {ex.Message} - for chunk: {chunk}");
                        continue;
                    }
                }

                // Add questions to the final response
                quizResponse.Questions = allQuestions;

                return quizResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new QuizResponse
                {
                    Questions = allQuestions,
                    Type = "Error",
                    Topic = "Error",
                    Description = "An error occurred while generating the quiz."
                };
            }
        }

        public async Task<QuizResponse> LoadQuizFromFileAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters = { new QuizQuestionTypeConverter() }
                };
                var quizResponse = JsonSerializer.Deserialize<QuizResponse>(json, options);
                return quizResponse ?? new QuizResponse
                {
                    Type = "File",
                    Topic = "Loaded Quiz",
                    Description = "This quiz was loaded from a file."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return new QuizResponse
                {
                    Type = "Error",
                    Topic = "Error",
                    Description = "An error occurred while loading the quiz from the file."
                };
            }
        }

        private List<string> SplitContent(string content, int chunkSize)
        {
            var chunks = new List<string>();
            if (string.IsNullOrEmpty(content)) return chunks;
            for (int i = 0; i < content.Length; i += chunkSize)
            {
                chunks.Add(content.Substring(i, Math.Min(chunkSize, content.Length - i)));
            }
            return chunks;
        }
    }
}
