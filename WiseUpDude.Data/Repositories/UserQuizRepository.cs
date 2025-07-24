using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizRepository : IUserQuizRepository<WiseUpDude.Model.Quiz>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UserQuizRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<WiseUpDude.Model.Quiz>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizzes = await context.UserQuizzes
                .Include(uq => uq.Questions)
                .Include(uq => uq.User)
                .ToListAsync();

            return userQuizzes.Select(uq => new WiseUpDude.Model.Quiz
            {
                Id = uq.Id,
                Name = uq.Name,
                UserName = uq.User?.UserName ?? "Unknown User", // Handle possible null reference
                UserId = uq.User?.Id ?? "Unknown User Id",
                Questions = uq.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Options = string.IsNullOrEmpty(q.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty,
                    Citation = string.IsNullOrEmpty(q.CitationJson) ? new List<WiseUpDude.Model.CitationMeta>() : System.Text.Json.JsonSerializer.Deserialize<List<WiseUpDude.Model.CitationMeta>>(q.CitationJson),
                    ContextSnippet = q.ContextSnippet 
                }).ToList(),
                Type = uq.Type,
                Topic = uq.Topic,
                Prompt = uq.Prompt,
                Description = uq.Description,
                Url = uq.Url,
                Difficulty = uq.Difficulty, // Include quiz-level difficulty
                LearnMode = uq.LearnMode // Include LearnMode
            });
        }

        public async Task<WiseUpDude.Model.Quiz> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = await context.UserQuizzes
                .Include(uq => uq.Questions)
                .Include(uq => uq.User)
                .FirstOrDefaultAsync(uq => uq.Id == id);

            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {id} not found.");

            return new WiseUpDude.Model.Quiz
            {
                Id = userQuiz.Id,
                Name = userQuiz.Name,
                UserName = userQuiz.User?.UserName ?? "Unknown User", // Handle possible null reference
                UserId = userQuiz.User?.Id ?? "Unknown User Id",
                Questions = userQuiz.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Options = string.IsNullOrEmpty(q.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty, // Include question-level difficulty
                    Citation = string.IsNullOrEmpty(q.CitationJson) ? new List<WiseUpDude.Model.CitationMeta>() : System.Text.Json.JsonSerializer.Deserialize<List<WiseUpDude.Model.CitationMeta>>(q.CitationJson),
                    ContextSnippet = q.ContextSnippet 
                }).ToList(),
                Type = userQuiz.Type,
                Topic = userQuiz.Topic,
                Prompt = userQuiz.Prompt,
                Description = userQuiz.Description,
                Url = userQuiz.Url,
                Difficulty = userQuiz.Difficulty, // Include quiz-level difficulty
                LearnMode = userQuiz.LearnMode // Include LearnMode
            };
        }

        public async Task AddAsync(WiseUpDude.Model.Quiz quiz)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = new UserQuiz
            {
                Name = quiz.Name,
                UserId = quiz.UserId,
                Type = quiz.Type,
                Topic = quiz.Topic,
                Prompt = quiz.Prompt,
                Description = quiz.Description,
                Url = quiz.Url,
                Difficulty = quiz.Difficulty, // Save quiz-level difficulty
                LearnMode = quiz.LearnMode, // Save LearnMode
                Questions = quiz.Questions.Select(q => new UserQuizQuestion
                {
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (UserQuizQuestionType)q.QuestionType,
                    OptionsJson = q.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(q.Options),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty, // Save question-level difficulty
                    CitationJson = q.Citation == null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(q.Citation),
                    ContextSnippet = q.ContextSnippet,
                    Quiz = null // Will be set before saving
                }).ToList()
            };

            // Set the Quiz property for each question before saving
            foreach (var question in userQuiz.Questions)
            {
                question.Quiz = userQuiz;
            }

            context.UserQuizzes.Add(userQuiz);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddAsyncGetId(Model.Quiz quiz)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = new UserQuiz
            {
                Name = quiz.Name,
                UserId = quiz.UserId,
                Type = quiz.Type,
                Topic = quiz.Topic,
                Prompt = quiz.Prompt,
                Description = quiz.Description,
                Url = quiz.Url,
                Difficulty = quiz.Difficulty,
                LearnMode = quiz.LearnMode,
                Questions = quiz.Questions.Select(q => new UserQuizQuestion
                {
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (UserQuizQuestionType)q.QuestionType,
                    OptionsJson = q.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(q.Options),
                    UserAnswer = q.UserAnswer,
                    Difficulty = q.Difficulty,
                    CitationJson = q.Citation == null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(q.Citation),
                    ContextSnippet = q.ContextSnippet, // Include ContextSnippet if needed
                    Quiz = null
                }).ToList()
            };

            foreach (var question in userQuiz.Questions)
            {
                question.Quiz = userQuiz;
            }

            context.UserQuizzes.Add(userQuiz);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log or rethrow for debugging
                throw new InvalidOperationException("Failed to save UserQuiz and UserQuizQuestions", ex);
            }

            return userQuiz.Id;
        }

        public async Task UpdateAsync(WiseUpDude.Model.Quiz quiz)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = await context.UserQuizzes
                .Include(uq => uq.Questions)
                .FirstOrDefaultAsync(uq => uq.Id == quiz.Id);

            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {quiz.Id} not found.");

            userQuiz.Name = quiz.Name;
            userQuiz.Type = quiz.Type;
            userQuiz.Topic = quiz.Topic;
            userQuiz.Prompt = quiz.Prompt;
            userQuiz.Description = quiz.Description;
            userQuiz.Url = quiz.Url;
            userQuiz.Difficulty = quiz.Difficulty; // Update quiz-level difficulty
            userQuiz.LearnMode = quiz.LearnMode; // Update LearnMode

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
                CitationJson = q.Citation == null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(q.Citation),
                ContextSnippet = q.ContextSnippet,
                Quiz = userQuiz // Set the Quiz property
            }));

            context.UserQuizzes.Update(userQuiz);
            await context.SaveChangesAsync();
        }

        public async Task UpdateQuizNameAsync(int id, string newName)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            // Find the UserQuiz by ID
            var userQuiz = await context.UserQuizzes.FirstOrDefaultAsync(uq => uq.Id == id);

            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {id} not found.");

            // Update the name
            userQuiz.Name = newName;

            // Save changes to the database
            await context.SaveChangesAsync();
        }

        public async Task UpdateLearnModeAsync(int id, bool learnMode)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = await context.UserQuizzes.FirstOrDefaultAsync(uq => uq.Id == id);

            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {id} not found.");

            userQuiz.LearnMode = learnMode;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = await context.UserQuizzes.FindAsync(id);
            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {id} not found.");

            context.UserQuizzes.Remove(userQuiz);
            await context.SaveChangesAsync();
        }

        public async Task<List<WiseUpDude.Model.Quiz>> GetUserQuizzesAsync(string userId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizzes = await context.UserQuizzes
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
                    Difficulty = q.Difficulty,
                    Citation = string.IsNullOrEmpty(q.CitationJson)
                        ? new List<Model.CitationMeta>()
                        : TryDeserializeCitation(q.CitationJson),
                    ContextSnippet = q.ContextSnippet
                }).ToList(),
                Type = uq.Type,
                Topic = uq.Topic,
                Prompt = uq.Prompt,
                Description = uq.Description,
                Difficulty = uq.Difficulty,
                LearnMode = uq.LearnMode, // Include LearnMode
                CreationDate = uq.CreationDate, // Include CreationDate for filtering
                                          //TopicId = uq.TopicId // Assuming TopicId exists in the entity
                UserId = uq.User?.Id ?? "Unknown User Id",
            }).ToList();
        }


        // Helper to get recent quizzes (e.g., last 5)
        public async Task<List<RecentQuizDto>> GetRecentUserQuizzesAsync(string userId, int count = 5)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizzes = await context.UserQuizzes
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
                Description = uq.Description,
                LearnMode = uq.LearnMode // Include LearnMode
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

        private List<Model.CitationMeta> TryDeserializeCitation(string json)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<Model.CitationMeta>>(json) ?? new List<Model.CitationMeta>();
            }
            catch
            {
                // Optionally log the bad JSON here for diagnostics
                return new List<Model.CitationMeta>();
            }
        }
    }
}
