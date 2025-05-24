using Microsoft.Extensions.Logging;
using WiseUpDude.Model;
using WiseUpDude.Shared;


namespace WiseUpDude.Shared.Services
{
    public class QuizAttemptService
    {
        private readonly UserQuizAttemptApiService _attemptApi;
        private readonly UserQuizAttemptQuestionApiService _questionApi;
        private readonly ILogger<QuizAttemptService> _logger;

        public QuizAttemptService(
            UserQuizAttemptApiService attemptApi,
            UserQuizAttemptQuestionApiService questionApi,
            ILogger<QuizAttemptService> logger)
        {
            _attemptApi = attemptApi;
            _questionApi = questionApi;
            _logger = logger;
        }

        public async Task<UserQuizAttempt?> CreateAttemptAsync(int quizId, int totalAnswered, int correctAnswers)
        {
            try
            {
                var attempt = new UserQuizAttempt
                {
                    UserQuizId = quizId,
                    AttemptDate = DateTime.Now,
                    Score = totalAnswered > 0 ? (double)correctAnswers / totalAnswered : 0,
                    Duration = TimeSpan.Zero
                };
                var created = await _attemptApi.CreateAsync(attempt);
                return created?.Id > 0 ? created : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create attempt");
                return null;
            }
        }

        public async Task<bool> UpdateAttemptAsync(WiseUpDude.Model.UserQuizAttempt attempt, int totalAnswered, int correctAnswers, DateTime startTime)
        {
            try
            {
                attempt.Score = totalAnswered > 0 ? (double)correctAnswers / totalAnswered : 0;
                attempt.Duration = DateTime.Now - startTime;
                await _attemptApi.UpdateAsync(attempt);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update attempt");
                return false;
            }
        }

        public async Task<bool> SaveAnswerAsync(WiseUpDude.Model.UserQuizAttempt attempt, QuizQuestion question, bool isCorrect)
        {
            try
            {
                var userAttemptQuestion = new UserQuizAttemptQuestion
                {
                    Id = 0,
                    UserQuizAttemptId = attempt.Id,
                    UserQuizQuestionId = question.Id,
                    UserAnswer = question.UserAnswer ?? string.Empty,
                    IsCorrect = isCorrect,
                    TimeTakenSeconds = null
                };
                var result = await _questionApi.CreateOrUpdateAsync(userAttemptQuestion);
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save answer");
                return false;
            }
        }
    }
}