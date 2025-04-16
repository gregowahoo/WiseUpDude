using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
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
                .Include(q => q.QuizSource) // Include the QuizSource to map it
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
                QuizSource = new Model.QuizSource // Map the QuizSource entity
                {
                    Id = e.QuizSource?.Id ?? 0, // Handle possible null reference
                    Type = e.QuizSource?.Type ?? "Unknown Type",
                    Topic = e.QuizSource?.Topic ?? "Unknown Topic",
                    Prompt = e.QuizSource?.Prompt,
                    Description = e.QuizSource?.Description ?? "No description available."
                }
            });
        }

        public async Task<Model.Quiz> GetByIdAsync(int id)
        {
            var entity = await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User to access UserName
                .Include(q => q.QuizSource) // Include the QuizSource to map it
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
                QuizSource = new Model.QuizSource // Map the QuizSource entity
                {
                    Id = entity.QuizSource?.Id ?? 0, // Handle possible null reference
                    Type = entity.QuizSource?.Type ?? "Unknown Type",
                    Topic = entity.QuizSource?.Topic ?? "Unknown Topic",
                    Prompt = entity.QuizSource?.Prompt,
                    Description = entity.QuizSource?.Description ?? "No description available."
                }
            };
        }

        public async Task AddAsync(Model.Quiz quiz)
        {
            // Create a new QuizSource entity
            var quizSource = new Entities.QuizSource
            {
                Type = quiz.QuizSource?.Type ?? "Unknown Type",
                Topic = quiz.QuizSource?.Topic,
                Prompt = quiz.QuizSource?.Prompt,
                Description = quiz.QuizSource?.Description
            };

            // Add the QuizSource to the database
            _context.QuizSources.Add(quizSource);
            await _context.SaveChangesAsync();

            // Create a new Quiz entity
            var entity = new Entities.Quiz
            {
                Name = quiz.Name,
                Questions = quiz.Questions.Select(q => new Entities.QuizQuestion
                {
                    Question = q.Question,
                    QuestionType = (Entities.QuizQuestionType)q.QuestionType,
                    OptionsJson = q.Options != null ? System.Text.Json.JsonSerializer.Serialize(q.Options) : null,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    UserAnswer = q.UserAnswer
                }).ToList(),
                QuizSourceId = quizSource.Id, // Link the QuizSource
                UserId = quiz.User.Id // Use the User.Id property
            };

            // Add the Quiz to the database
            _context.Quizzes.Add(entity);
            await _context.SaveChangesAsync();

            quiz.Id = entity.Id;
        }


        public async Task UpdateAsync(Model.Quiz model)
        {
            var entity = await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.QuizSource) // Include QuizSource for updating
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {model.Id} not found.");

            // Update Quiz properties
            entity.Name = model.Name;
            entity.Questions = model.Questions.Select(q => new Data.Entities.QuizQuestion
            {
                Id = q.Id,
                Question = q.Question,
                Answer = q.Answer
            }).ToList();

            // Update QuizSource properties
            if (entity.QuizSource != null && model.QuizSource != null)
            {
                entity.QuizSource.Type = model.QuizSource.Type;
                entity.QuizSource.Topic = model.QuizSource.Topic;
                entity.QuizSource.Prompt = model.QuizSource.Prompt;
                entity.QuizSource.Description = model.QuizSource.Description;

                _context.Entry(entity.QuizSource).State = EntityState.Modified;
            }

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Find the quiz by its ID
            var quiz = await _context.Quizzes
                .Include(q => q.QuizSource) // Include the associated QuizSource
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz != null)
            {
                // Remove the associated QuizSource
                if (quiz.QuizSource != null)
                {
                    _context.QuizSources.Remove(quiz.QuizSource);
                }

                // Remove the quiz
                _context.Quizzes.Remove(quiz);

                // Save changes to the database
                await _context.SaveChangesAsync();
            }
        }
    }
}

