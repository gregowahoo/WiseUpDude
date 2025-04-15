using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

        public async Task<QuizResponse> GenerateQuizAsync(string content)
        {
            var finalQuizResponse = new QuizResponse
            {
                QuizSource = new QuizSource
                {
                    Type = "Generated",
                    Topic = "AI-Generated Quiz",
                    Description = "This quiz was generated based on the provided content."
                }
            };
            var allQuestions = new List<QuizQuestion>();

            try
            {
                int chunkSize = 4000;
                var chunks = SplitContent(content, chunkSize);
                bool isFirstChunk = true;

                foreach (var chunk in chunks)
                {
                    string prompt = isFirstChunk
                        ? $"Generate a quiz based on the following content:\n\n{chunk}\n\n" +
                          "Include both true/false questions and multiple-choice questions. " +
                          "For each question, output the following fields in JSON: " +
                          "'Question', 'Options', 'Answer', 'Explanation'. " +
                          "Return only valid JSON with root object 'Questions' as an array." +
                          "Return only the raw JSON without any code block formatting or prefixes like 'json'."
                        : $"Continue generating quiz questions based on the following content:\n\n{chunk}\n\n" +
                          "Maintain the style, format, and JSON structure from the previous questions. " +
                          "For each question, output the following fields in JSON: " +
                          "'Question', 'Options', 'Answer', 'Explanation'. " +
                          "Return only valid JSON with root object 'Questions' as an array." +
                          "Return only the raw JSON without any code block formatting or prefixes like 'json'.";

                    isFirstChunk = false;

                    var result = await _chatClient.GetResponseAsync(prompt);
                    var quizJson = result.Text;

                    try
                    {
                        // Validate JSON string
                        using (JsonDocument doc = JsonDocument.Parse(quizJson))
                        {
                            var chunkQuizResponse = JsonSerializer.Deserialize<QuizResponse>(quizJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (chunkQuizResponse?.Questions != null)
                            {
                                allQuestions.AddRange(chunkQuizResponse.Questions);
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
                finalQuizResponse.Questions = allQuestions;

                return finalQuizResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new QuizResponse
                {
                    Questions = allQuestions,
                    QuizSource = new QuizSource
                    {
                        Type = "Error",
                        Topic = "Error",
                        Description = "An error occurred while generating the quiz."
                    }
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
                    QuizSource = new QuizSource
                    {
                        Type = "File",
                        Topic = "Loaded Quiz",
                        Description = "This quiz was loaded from a file."
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return new QuizResponse
                {
                    QuizSource = new QuizSource
                    {
                        Type = "Error",
                        Topic = "Error",
                        Description = "An error occurred while loading the quiz from the file."
                    }
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
