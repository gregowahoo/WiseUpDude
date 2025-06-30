using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using Microsoft.Extensions.Logging;

namespace WiseUpDude.Services
{
    public class LearningTrackQuizService
    {
        private readonly ILearningTrackQuizRepository _quizRepository;
        private readonly ILearningTrackQuizQuestionRepository _questionRepository;
        private readonly ILearningTrackSourceRepository _sourceRepository;
        private readonly PerplexityService _perplexityService;
        private readonly ILogger<LearningTrackQuizService> _logger;

        public LearningTrackQuizService(
            ILearningTrackQuizRepository quizRepository,
            ILearningTrackQuizQuestionRepository questionRepository,
            ILearningTrackSourceRepository sourceRepository,
            PerplexityService perplexityService,
            ILogger<LearningTrackQuizService> logger)
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _sourceRepository = sourceRepository;
            _perplexityService = perplexityService;
            _logger = logger;
        }

        public async Task<(LearningTrackQuiz? Quiz, string? Error)> GenerateAndPersistQuizFromUrlAsync(string url, int learningTrackSourceId)
        {
            var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(url);
            var sourceName = await GetSourceNameAsync(learningTrackSourceId);
            var (json, apiError) = await _perplexityService.GetPerplexityQuizJsonAsync(aiPrompt);
            if (apiError != null)
                return (null, apiError);
            var (quizModel, parseError) = _perplexityService.ParseQuizJson(json);
            if (parseError != null)
                return (null, parseError);
            if (quizModel == null || quizModel.Questions == null || !quizModel.Questions.Any())
                return (null, "No quiz questions found in Perplexity response.");
            var quiz = await CreateAndPersistLearningTrackQuiz(quizModel, sourceName, learningTrackSourceId);
            return (quiz, null);
        }

        private async Task<string> GetSourceNameAsync(int learningTrackSourceId)
        {
            var source = await _sourceRepository.GetByIdAsync(learningTrackSourceId);
            return source?.Name ?? "Unknown Source";
        }

        private async Task<LearningTrackQuiz> CreateAndPersistLearningTrackQuiz(Quiz quizModel, string sourceName, int learningTrackSourceId)
        {
            var quiz = new LearningTrackQuiz
            {
                Name = sourceName,
                Description = quizModel.Description,
                LearningTrackSourceId = learningTrackSourceId,
                CreationDate = DateTime.UtcNow,
                Questions = quizModel.Questions.Select(q => new LearningTrackQuizQuestion
                {
                    Question = q.Question ?? string.Empty,
                    Answer = q.Answer ?? string.Empty,
                    Explanation = q.Explanation,
                    OptionsJson = q.OptionsJson,
                    Difficulty = q.Difficulty,
                    CreationDate = DateTime.UtcNow
                }).ToList()
            };
            await _quizRepository.AddQuizAsync(quiz);
            return quiz;
        }
    }
}
