using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data;
using WiseUpDude.Model;
using System.Text.Json;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class QuizRepository : IRepository<WiseUpDude.Model.Quiz>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public QuizRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<WiseUpDude.Model.Quiz>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User to access UserName
                .Include(q => q.Topic) // Include the Topic
                .ToListAsync();

            return entities.Select(e => new WiseUpDude.Model.Quiz
            {
                Id = e.Id,
                Name = e.Name,
                UserName = e.User?.UserName ?? "Unknown User", // Handle possible null reference
                UserId = e.User?.Id ?? "Unknown User Id",
                Questions = e.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Options = q.OptionsJson != null ? JsonSerializer.Deserialize<List<string>>(q.OptionsJson) : null,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Difficulty = q.Difficulty
                }).ToList(),
                Type = e.Type,
                Topic = e.Topic?.Name, // Use the Topic's Name
                TopicId = (int)e.TopicId, // Map TopicId
                Description = e.Description,
                Difficulty = e.Difficulty, // Include quiz-level difficulty
                CreationDate = e.CreationDate,
                IsQuizOfTheDay = e.IsQuizOfTheDay,
                QuizOfTheDayDate = e.QuizOfTheDayDate
            });
        }

        public async Task<WiseUpDude.Model.Quiz> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User to access UserName
                .Include(q => q.Topic) // Include the Topic
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {id} not found.");

            return new WiseUpDude.Model.Quiz
            {
                Id = entity.Id,
                Name = entity.Name,
                UserName = entity.User?.UserName ?? "Unknown User", // Handle possible null reference
                UserId = entity.User?.Id ?? "Unknown User Id",
                Questions = entity.Questions.Select(q => new WiseUpDude.Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Options = q.OptionsJson != null ? JsonSerializer.Deserialize<List<string>>(q.OptionsJson) : null,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)q.QuestionType,
                    Difficulty = q.Difficulty
                }).ToList(),
                Type = entity.Type,
                Topic = entity.Topic?.Name, // Use the Topic's Name
                TopicId = entity.TopicId, // Map TopicId
                Description = entity.Description,
                Difficulty = entity.Difficulty, // Include quiz-level difficulty
                CreationDate = entity.CreationDate,
                IsQuizOfTheDay = entity.IsQuizOfTheDay,
                QuizOfTheDayDate = entity.QuizOfTheDayDate
            };
        }

        public async Task<IEnumerable<WiseUpDude.Model.Quiz>> GetQuizzesByTopicIdAsync(int topicId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var quizzes = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include User for UserName
                .Include(q => q.Topic) // Include Topic for Topic details
                .Where(q => q.TopicId == topicId) // Filter by TopicId
                .ToListAsync();

            return quizzes.Select(q => new WiseUpDude.Model.Quiz
            {
                Id = q.Id,
                Name = q.Name,
                Questions = q.Questions.Select(qq => new WiseUpDude.Model.QuizQuestion
                {
                    Id = qq.Id,
                    Question = qq.Question,
                    Options = qq.OptionsJson != null ? JsonSerializer.Deserialize<List<string>>(qq.OptionsJson) : null,
                    Answer = qq.Answer,
                    Explanation = qq.Explanation,
                    QuestionType = (WiseUpDude.Model.QuizQuestionType)qq.QuestionType,
                    Difficulty = qq.Difficulty
                }).ToList(),
                UserName = q.User?.UserName ?? "Unknown User",
                UserId = q.User?.Id ?? "Unknown User",
                Type = q.Type,
                Topic = q.Topic?.Name,
                TopicId = q.TopicId,
                Prompt = q.Prompt,
                Description = q.Description,
                Difficulty = q.Difficulty,
                CreationDate = q.CreationDate,
                IsQuizOfTheDay = q.IsQuizOfTheDay,
                QuizOfTheDayDate = q.QuizOfTheDayDate
            });
        }


        public async Task AddAsync(WiseUpDude.Model.Quiz quiz)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            // Create a new Quiz entity
            var entity = new Entities.Quiz
            {
                Name = quiz.Name,
                UserId = quiz.UserId,
                Type = quiz.Type,
                Prompt = quiz.Prompt,
                TopicId = quiz.TopicId, // Use TopicId directly
                Description = quiz.Description,
                Difficulty = quiz.Difficulty,
                CreationDate = quiz.CreationDate != default ? quiz.CreationDate : DateTime.UtcNow,
                IsQuizOfTheDay = quiz.IsQuizOfTheDay,
                QuizOfTheDayDate = quiz.QuizOfTheDayDate
            };

            // Add the Quiz to the database first to generate its ID
            context.Quizzes.Add(entity);
            await context.SaveChangesAsync();

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
                Quiz = entity, // Set the required Quiz property
                Difficulty = q.Difficulty // Save question-level difficulty
            }).ToList();

            // Update the Quiz entity with its questions
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();

            quiz.Id = entity.Id;
        }

        public async Task<int> AddQuizAsync(QuizResponse quizResponse)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var quizName = string.IsNullOrWhiteSpace(quizResponse.Topic)
                ? $"Quiz_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                : quizResponse.Topic;

            //var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            //if (user == null)
            //{
            //    throw new InvalidOperationException($"User with UserName '{userName}' not found.");
            //}

            // Find the Topic by its Name
            var topic = await context.Topics.FirstOrDefaultAsync(t => t.Name == quizResponse.Topic);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with Name '{quizResponse.Topic}' not found.");

            var quiz = new Entities.Quiz
            {
                Name = quizName,
                UserId = quizResponse.UserId,
                Type = quizResponse.Type,
                TopicId = topic.Id, // Set the TopicId
                Description = quizResponse.Description,
                Difficulty = quizResponse.Difficulty // Save quiz-level difficulty
            };

            // Add the Quiz to the database first to generate its ID
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            // Add Questions with the Quiz reference
            quiz.Questions = quizResponse.Questions.Select(q => new Entities.QuizQuestion
            {
                Question = q.Question,
                QuestionType = (Entities.QuizQuestionType)q.QuestionType,
                OptionsJson = System.Text.Json.JsonSerializer.Serialize(q.Options),
                Answer = q.Answer,
                Explanation = q.Explanation,
                QuizId = quiz.Id, // Set the QuizId
                Quiz = quiz, // Set the required Quiz property
                Difficulty = q.Difficulty // Save question-level difficulty
            }).ToList();

            // Update the Quiz entity with its questions
            context.Entry(quiz).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return quiz.Id;
        }

        public async Task UpdateAsync(WiseUpDude.Model.Quiz model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == model.Id);

            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {model.Id} not found.");

            entity.Name = model.Name;
            entity.Type = model.Type;
            entity.TopicId = model.TopicId; // Update TopicId
            entity.Description = model.Description;
            entity.Difficulty = model.Difficulty; // Update quiz-level difficulty
            entity.IsQuizOfTheDay = model.IsQuizOfTheDay;
            entity.QuizOfTheDayDate = model.QuizOfTheDayDate;

            // Update Questions with the Quiz reference
            entity.Questions = model.Questions.Select(q => new Entities.QuizQuestion
            {
                Id = q.Id,
                Question = q.Question,
                Answer = q.Answer,
                QuizId = entity.Id, // Set the QuizId
                Quiz = entity, // Set the required Quiz property
                Difficulty = q.Difficulty // Update question-level difficulty
            }).ToList();

            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            // Find the quiz by its ID
            var quiz = await context.Quizzes
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz != null)
            {
                // Remove the quiz
                context.Quizzes.Remove(quiz);

                // Save changes to the database
                await context.SaveChangesAsync();
            }
        }

    }
}
