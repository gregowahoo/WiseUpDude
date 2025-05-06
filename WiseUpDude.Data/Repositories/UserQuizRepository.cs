using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizRepository : IUserRepository<WiseUpDude.Model.Quiz>
    {
        private readonly ApplicationDbContext _context;

        public UserQuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WiseUpDude.Model.Quiz>> GetAllAsync()
        {
            var userQuizzes = await _context.UserQuizzes
                .Include(uq => uq.Questions)
                .Include(uq => uq.User)
                .ToListAsync();

            return userQuizzes.Select(uq => new WiseUpDude.Model.Quiz
            {
                Id = uq.Id,
                Name = uq.Name,
                UserName = uq.User.UserName,
                User = new ApplicationUser
                {
                    Id = uq.User.Id,
                    UserName = uq.User.UserName
                },
                Questions = uq.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Options = string.IsNullOrEmpty(q.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty // Include question-level difficulty
                }).ToList(),
                Type = uq.Type,
                Topic = uq.Topic,
                Prompt = uq.Prompt,
                Description = uq.Description,
                Difficulty = uq.Difficulty // Include quiz-level difficulty
            });
        }

        public async Task<WiseUpDude.Model.Quiz> GetByIdAsync(int id)
        {
            var userQuiz = await _context.UserQuizzes
                .Include(uq => uq.Questions)
                .Include(uq => uq.User)
                .FirstOrDefaultAsync(uq => uq.Id == id);

            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {id} not found.");

            return new WiseUpDude.Model.Quiz
            {
                Id = userQuiz.Id,
                Name = userQuiz.Name,
                UserName = userQuiz.User.UserName,
                User = new ApplicationUser
                {
                    Id = userQuiz.User.Id,
                    UserName = userQuiz.User.UserName
                },
                Questions = userQuiz.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Options = string.IsNullOrEmpty(q.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty // Include question-level difficulty
                }).ToList(),
                Type = userQuiz.Type,
                Topic = userQuiz.Topic,
                Prompt = userQuiz.Prompt,
                Description = userQuiz.Description,
                Difficulty = userQuiz.Difficulty // Include quiz-level difficulty
            };
        }

        //public Task<IEnumerable<Model.Topic>> GetTopicsAsync(int count)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task AddAsync(WiseUpDude.Model.Quiz quiz)
        {
            var userQuiz = new UserQuiz
            {
                Name = quiz.Name,
                UserId = quiz.User.Id,
                Type = quiz.Type,
                Topic = quiz.Topic,
                Prompt = quiz.Prompt,
                Description = quiz.Description,
                Difficulty = quiz.Difficulty, // Save quiz-level difficulty
                Questions = quiz.Questions.Select(q => new UserQuizQuestion
                {
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (UserQuizQuestionType)q.QuestionType,
                    OptionsJson = q.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(q.Options),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty, // Save question-level difficulty
                    Quiz = null // Will be set before saving
                }).ToList()
            };

            // Set the Quiz property for each question before saving
            foreach (var question in userQuiz.Questions)
            {
                question.Quiz = userQuiz;
            }

            _context.UserQuizzes.Add(userQuiz);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WiseUpDude.Model.Quiz quiz)
        {
            var userQuiz = await _context.UserQuizzes
                .Include(uq => uq.Questions)
                .FirstOrDefaultAsync(uq => uq.Id == quiz.Id);

            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {quiz.Id} not found.");

            userQuiz.Name = quiz.Name;
            userQuiz.Type = quiz.Type;
            userQuiz.Topic = quiz.Topic;
            userQuiz.Prompt = quiz.Prompt;
            userQuiz.Description = quiz.Description;
            userQuiz.Difficulty = quiz.Difficulty; // Update quiz-level difficulty

            // Update questions
            userQuiz.Questions.Clear();
            userQuiz.Questions.AddRange(quiz.Questions.Select(q => new UserQuizQuestion
            {
                Question = q.Question,
                Answer = q.Answer,
                Explanation = q.Explanation,
                QuestionType = (UserQuizQuestionType)q.QuestionType,
                OptionsJson = q.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(q.Options),
                UserAnswer = q.UserAnswer,
                Difficulty = q.Difficulty, // Update question-level difficulty
                Quiz = userQuiz // Set the Quiz property
            }));

            _context.UserQuizzes.Update(userQuiz);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var userQuiz = await _context.UserQuizzes.FindAsync(id);
            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {id} not found.");

            _context.UserQuizzes.Remove(userQuiz);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WiseUpDude.Model.Quiz>> GetUserQuizzesAsync(string userId)
        {
            var userQuizzes = await _context.UserQuizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Eagerly load the User property
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreationDate)
                .ToListAsync();

            return userQuizzes.Select(uq => new WiseUpDude.Model.Quiz
            {
                Id = uq.Id,
                Name = uq.Name,
                UserName = uq.UserId, // Assuming UserId maps to UserName in the model
                Questions = uq.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Options = string.IsNullOrEmpty(q.OptionsJson)
                        ? new List<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty
                }).ToList(),
                Type = uq.Type,
                Topic = uq.Topic,
                Prompt = uq.Prompt,
                Description = uq.Description,
                Difficulty = uq.Difficulty,
                //TopicId = uq.TopicId // Assuming TopicId exists in the entity
                User = new ApplicationUser
                {
                    Id = uq.UserId,
                    UserName = uq.User.UserName
                }
            }).ToList();
        }


        // Helper to get recent quizzes (e.g., last 5)
        public async Task<List<RecentQuizDto>> GetRecentUserQuizzesAsync(string userId, int count = 5)
        {
            var userQuizzes = await _context.UserQuizzes
                .Include(q => q.Questions)
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreationDate)
                .Take(count)
                .ToListAsync();

            return userQuizzes.Select(uq => new RecentQuizDto
            {
                Id = uq.Id, // Populate the Id
                Name = uq.Name,
                Score = CalculateScore(uq),
                Type = uq.Type,
                Topic = uq.Topic,
                Prompt = uq.Prompt,
                Description = uq.Description
            }).ToList();
        }
        private double CalculateScore(UserQuiz userQuiz)
        {
            if (userQuiz.Questions == null || userQuiz.Questions.Count == 0)
                return 0;

            int totalQuestions = userQuiz.Questions.Count;
            int correctAnswers = userQuiz.Questions.Count(q => q.UserAnswer == q.Answer);

            return (double)correctAnswers / totalQuestions * 100;
        }

    }
}
