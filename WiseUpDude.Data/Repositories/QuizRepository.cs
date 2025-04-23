using System.Collections.Generic;
// Ensure this repository is used only for function-generated quizzes.
// No changes needed here unless specific logic is required to differentiate usage.
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class QuizRepository : IRepository<Model.Quiz>
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Model.Quiz>> GetAllAsync()
        {
            var entities = await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User to access UserName
                .ToListAsync();

            return entities.Select(e => new Model.Quiz
            {
                Id = e.Id,
                Name = e.Name,
                UserName = e.User?.UserName ?? "Unknown User", // Handle possible null reference
                User = e.User ?? new ApplicationUser { UserName = "Unknown User" }, // Provide a default User object
                Questions = e.Questions.Select(q => new Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer
                }).ToList(),
                Type = e.Type,
                Topic = e.Topic,
                Prompt = e.Prompt,
                Description = e.Description
            });
        }

        public async Task<Model.Quiz> GetByIdAsync(int id)
        {
            var entity = await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User to access UserName
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {id} not found.");

            return new Model.Quiz
            {
                Id = entity.Id,
                Name = entity.Name,
                UserName = entity.User?.UserName ?? "Unknown User", // Handle possible null reference
                User = entity.User ?? new ApplicationUser { UserName = "Unknown User" }, // Provide a default User object
                Questions = entity.Questions.Select(q => new Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer
                }).ToList(),
                Type = entity.Type,
                Topic = entity.Topic,
                Prompt = entity.Prompt,
                Description = entity.Description
            };
        }

        public async Task AddAsync(Model.Quiz quiz)
        {
            // Create a new Quiz entity
            var entity = new Entities.Quiz
            {
                Name = quiz.Name,
                UserId = quiz.User.Id,
                Type = quiz.Type,
                Topic = quiz.Topic,
                Prompt = quiz.Prompt,
                Description = quiz.Description
            };

            // Add the Quiz to the database first to generate its ID
            _context.Quizzes.Add(entity);
            await _context.SaveChangesAsync();

            // Add Questions with the Quiz reference
            entity.Questions = quiz.Questions.Select(q => new Entities.QuizQuestion
            {
                Question = q.Question,
                QuestionType = (Entities.QuizQuestionType)q.QuestionType,
                OptionsJson = q.Options != null ? System.Text.Json.JsonSerializer.Serialize(q.Options) : null,
                Answer = q.Answer,
                Explanation = q.Explanation,
                UserAnswer = q.UserAnswer,
                QuizId = entity.Id, // Set the QuizId
                Quiz = entity // Set the required Quiz property
            }).ToList();

            // Update the Quiz entity with its questions
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            quiz.Id = entity.Id;
        }

        public async Task AddQuizAsync(QuizResponse quizResponse, string userName = "greg.ohlsen@gmail.com")
        {
            var quizName = string.IsNullOrWhiteSpace(quizResponse.Topic)
                ? $"Quiz_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                : quizResponse.Topic;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                throw new InvalidOperationException($"User with UserName '{userName}' not found.");
            }

            var quiz = new Entities.Quiz
            {
                Name = quizName,
                UserId = user.Id,
                Type = quizResponse.Type,
                Topic = quizResponse.Topic,
                Prompt = quizResponse.Prompt,
                Description = quizResponse.Description
            };

            // Add the Quiz to the database first to generate its ID
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            // Add Questions with the Quiz reference
            quiz.Questions = quizResponse.Questions.Select(q => new Entities.QuizQuestion
            {
                Question = q.Question,
                QuestionType = (Entities.QuizQuestionType)q.QuestionType,
                OptionsJson = System.Text.Json.JsonSerializer.Serialize(q.Options),
                Answer = q.Answer,
                Explanation = q.Explanation,
                QuizId = quiz.Id, // Set the QuizId
                Quiz = quiz // Set the required Quiz property
            }).ToList();

            // Update the Quiz entity with its questions
            _context.Entry(quiz).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Model.Quiz model)
        {
            var entity = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {model.Id} not found.");

            entity.Name = model.Name;
            entity.Type = model.Type;
            entity.Topic = model.Topic;
            entity.Prompt = model.Prompt;
            entity.Description = model.Description;

            // Update Questions with the Quiz reference
            entity.Questions = model.Questions.Select(q => new Entities.QuizQuestion
            {
                Id = q.Id,
                Question = q.Question,
                Answer = q.Answer,
                QuizId = entity.Id, // Set the QuizId
                Quiz = entity // Set the required Quiz property
            }).ToList();

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Find the quiz by its ID
            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz != null)
            {
                // Remove the quiz
                _context.Quizzes.Remove(quiz);

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
        }
    }
}
