using WiseUpDude.Data.Repositories;
//using WiseUpDude.Services.Models; // Ensure this using is present
using WiseUpDude.Data.Entities;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class DashboardDataService
    {
        private readonly UserQuizRepository _quizRepository;

        public DashboardDataService(UserQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string userId)
        {
            var quizzes = await _quizRepository.GetUserQuizzesAsync(userId);

            var dashboardSummary = new DashboardSummaryDto
            {
                TotalQuizzesTaken = quizzes.Count
            };

            if (quizzes.Count > 0)
            {
                // Calculate scores for each quiz
                var quizScores = quizzes.Select(q => new
                {
                    Quiz = q,
                    Score = CalculateQuizScore(q)
                }).ToList();

                dashboardSummary.AverageScore = quizScores.Average(q => q.Score);

                var bestQuiz = quizScores.OrderByDescending(q => q.Score).First();
                dashboardSummary.BestQuizName = bestQuiz.Quiz.Name;
                dashboardSummary.BestQuizScore = bestQuiz.Score;
            }

            // Recent quizzes (e.g., last 5)
            var recentQuizzes = quizzes
                .OrderByDescending(q => q.CreationDate)
                .Select(q => new WiseUpDude.Model.RecentQuizDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    Score = CalculateQuizScore(q),
                    Type = q.Type,
                    Topic = q.Topic,
                    Prompt = q.Prompt,
                    Description = q.Description,
                    LearnMode = q.LearnMode
                })
                .ToList();

            //dashboardSummary.RecentQuizzes = recentQuizzes.Cast<WiseUpDude.Data.Model.RecentQuizDto>().ToList();
            dashboardSummary.RecentQuizzes = recentQuizzes;

            return dashboardSummary;
        }

        // Calculates the score as percentage of correct answers
        private double CalculateQuizScore(Model.Quiz quiz)
        {
            if (quiz.Questions == null || quiz.Questions.Count == 0)
                return 0;

            int total = quiz.Questions.Count;
            int correct = quiz.Questions.Count(q =>
                !string.IsNullOrEmpty(q.UserAnswer) &&
                string.Equals(q.UserAnswer?.Trim(), q.Answer?.Trim(), StringComparison.OrdinalIgnoreCase)
            );

            return total == 0 ? 0 : (correct * 100.0) / total;
        }
    }
}
