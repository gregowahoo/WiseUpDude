﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WiseUpDude.Model;
using WiseUpDude.Data;
using Microsoft.EntityFrameworkCore;

namespace WiseUpDude.Services
{
    public class QuizService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly ApplicationDbContext _dbContext;

        public QuizService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _httpClient = httpClient;
            _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API key is missing.");
            _dbContext = dbContext;
        }

        public async Task<QuizResponse> GenerateQuizAsync(string content)
        {
            var finalQuizResponse = new QuizResponse();
            var allQuestions = new List<QuizQuestion>();

            try
            {
                int chunkSize = 4000; // Adjust based on testing and token estimation
                var chunks = SplitContent(content, chunkSize);
                bool isFirstChunk = true;

                foreach (var chunk in chunks)
                {
                    string prompt;
                    if (isFirstChunk)
                    {
                        prompt = $"Generate a quiz based on the following content:\n\n{chunk}\n\n" +
                                 "Include both true/false questions and multiple-choice questions. " +
                                 "For each question, output the following fields in JSON: " +
                                 "'Question', 'Options', 'Answer', 'Explanation'. " +
                                 "Return only valid JSON with root object 'Questions' as an array.";
                        isFirstChunk = false;
                    }
                    else
                    {
                        prompt = $"Continue generating quiz questions based on the following content:\n\n{chunk}\n\n" +
                                 "Maintain the style, format, and JSON structure from the previous questions. " +
                                 "For each question, output the following fields in JSON: " +
                                 "'Question', 'Options', 'Answer', 'Explanation'. " +
                                 "Return only valid JSON with root object 'Questions' as an array.";
                    }

                    var requestBody = new
                    {
                        model = "gpt-4",
                        messages = new[]
                        {
                            new { role = "system", content = "You are an AI that generates quizzes." },
                            new { role = "user", content = prompt }
                        },
                        temperature = 0.7,
                        max_tokens = 1000 // Adjust as needed per chunk
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(requestBody),
                        Encoding.UTF8,
                        "application/json"
                    );

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);

                    var response = await _httpClient.PostAsync(OpenAiApiUrl, jsonContent);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // 1) Deserialize the top-level OpenAI response
                    var openAiResponse = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(jsonResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (openAiResponse == null || openAiResponse.Choices == null || openAiResponse.Choices.Count == 0)
                    {
                        continue; // Skip to the next chunk
                    }

                    // 2) Extract the quiz JSON from message.content
                    var quizJson = openAiResponse.Choices[0].Message.Content;

                    // 3) Deserialize the quiz JSON and aggregate the questions
                    try
                    {
                        var chunkQuizResponse = JsonSerializer.Deserialize<QuizResponse>(quizJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (chunkQuizResponse?.Questions != null)
                        {
                            allQuestions.AddRange(chunkQuizResponse.Questions);
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Handle potential JSON parsing issues, log the error, and possibly retry or skip the chunk
                        Console.WriteLine($"JSON Parsing Error: {ex.Message} - for chunk: {chunk}");
                        continue; // Or implement a retry mechanism if needed
                    }
                }

                // Combine all questions into a single QuizResponse
                finalQuizResponse = new QuizResponse { Questions = allQuestions };
                return finalQuizResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return finalQuizResponse = new QuizResponse { Questions = allQuestions };
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
                return quizResponse ?? new QuizResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return new QuizResponse();
            }
        }

        private List<string> SplitContent(string content, int chunkSize)
        {
            var chunks = new List<string>();
            if (string.IsNullOrEmpty(content))
            {
                return chunks;
            }

            for (int i = 0; i < content.Length; i += chunkSize)
            {
                chunks.Add(content.Substring(i, Math.Min(chunkSize, content.Length - i)));
            }
            return chunks;
        }

        public async Task SaveQuizAsync(QuizModel quiz)
        {
            _dbContext.Quizzes.Add(quiz);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<QuizModel?> GetQuizAsync(int quizId)
        {
            return await _dbContext.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task UpdateQuizAsync(QuizModel quiz)
        {
            _dbContext.Quizzes.Update(quiz);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _dbContext.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                _dbContext.Quizzes.Remove(quiz);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task AddQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _dbContext.QuizQuestions.Add(quizQuestion);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<QuizQuestion?> GetQuizQuestionAsync(int quizQuestionId)
        {
            return await _dbContext.QuizQuestions.FindAsync(quizQuestionId);
        }

        public async Task UpdateQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _dbContext.QuizQuestions.Update(quizQuestion);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteQuizQuestionAsync(int quizQuestionId)
        {
            var quizQuestion = await _dbContext.QuizQuestions.FindAsync(quizQuestionId);
            if (quizQuestion != null)
            {
                _dbContext.QuizQuestions.Remove(quizQuestion);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
