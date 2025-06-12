using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizRepository : ILearningTrackQuizRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackQuizRepository> _logger;
        public LearningTrackQuizRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackQuizRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Model.LearningTrackQuiz>> GetAllQuizzesAsync()
        {
            _logger.LogInformation("Getting all learning track quizzes");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTrackQuizzes
                .Include(q => q.Questions)
                .ToListAsync();

            return entities.Select(e => new Model.LearningTrackQuiz
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                LearningTrackSourceId = e.LearningTrackSourceId,
                CreationDate = e.CreationDate,
                Questions = e.Questions.Select(q => new Model.LearningTrackQuizQuestion
                {
                    Id = q.Id,
                    LearningTrackQuizId = q.LearningTrackQuizId,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    OptionsJson = q.OptionsJson,
                    Difficulty = q.Difficulty,
                    CreationDate = q.CreationDate
                }).ToList()
            });
        }

        public async Task<Model.LearningTrackQuiz?> GetQuizByIdAsync(int id)
        {
            _logger.LogInformation("Getting learning track quiz by Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity == null)
                return null;

            return new Model.LearningTrackQuiz
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                LearningTrackSourceId = entity.LearningTrackSourceId,
                CreationDate = entity.CreationDate,
                Questions = entity.Questions.Select(q => new Model.LearningTrackQuizQuestion
                {
                    Id = q.Id,
                    LearningTrackQuizId = q.LearningTrackQuizId,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    OptionsJson = q.OptionsJson,
                    Difficulty = q.Difficulty,
                    CreationDate = q.CreationDate
                }).ToList()
            };
        }

        public async Task<int> AddQuizAsync(Model.LearningTrackQuiz quiz)
        {
            _logger.LogInformation("Adding learning track quiz: {@Quiz}", quiz);
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            // Create a new LearningTrackQuiz entity
            var entity = new Entities.LearningTrackQuiz
            {
                Name = quiz.Name,
                Description = quiz.Description,
                LearningTrackSourceId = quiz.LearningTrackSourceId,
                CreationDate = quiz.CreationDate
            };

            // Add the quiz to the database first to generate its ID
            context.LearningTrackQuizzes.Add(entity);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully added learning track quiz with Id={Id}", entity.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding learning track quiz: {ErrorMessage}", ex.Message);
                throw;
            }

            // Add Questions with the Quiz reference
            entity.Questions = quiz.Questions.Select(q => new Entities.LearningTrackQuizQuestion
            {
                Question = q.Question,
                Answer = q.Answer,
                Explanation = q.Explanation,
                OptionsJson = q.OptionsJson,
                Difficulty = q.Difficulty,
                CreationDate = q.CreationDate,
                LearningTrackQuizId = entity.Id,
                LearningTrackQuiz = entity
            }).ToList();

            // Update the Quiz entity with its questions
            context.Entry(entity).State = EntityState.Modified;
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully added questions for quiz Id={Id}", entity.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding questions for quiz Id={Id}: {ErrorMessage}", entity.Id, ex.Message);
                throw;
            }

            return entity.Id;
        }

        public async Task UpdateQuizAsync(Model.LearningTrackQuiz quiz)
        {
            _logger.LogInformation("Updating learning track quiz Id={Id}", quiz.Id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quiz.Id);

            if (entity == null)
            {
                _logger.LogWarning("Learning track quiz Id={Id} not found for update", quiz.Id);
                return;
            }

            entity.Name = quiz.Name;
            entity.Description = quiz.Description;
            entity.LearningTrackSourceId = quiz.LearningTrackSourceId;
            entity.CreationDate = quiz.CreationDate;

            // Remove deleted questions
            var modelQuestionIds = quiz.Questions?.Select(q => q.Id).ToHashSet() ?? new HashSet<int>();
            var toRemove = entity.Questions.Where(q => !modelQuestionIds.Contains(q.Id)).ToList();
            foreach (var q in toRemove)
                context.LearningTrackQuizQuestions.Remove(q);

            // Update or add questions
            foreach (var qModel in quiz.Questions ?? Enumerable.Empty<Model.LearningTrackQuizQuestion>())
            {
                var qEntity = entity.Questions.FirstOrDefault(q => q.Id == qModel.Id);
                if (qEntity != null)
                {
                    // Update existing
                    qEntity.Question = qModel.Question;
                    qEntity.Answer = qModel.Answer;
                    qEntity.Explanation = qModel.Explanation;
                    qEntity.OptionsJson = qModel.OptionsJson;
                    qEntity.Difficulty = qModel.Difficulty;
                    qEntity.CreationDate = qModel.CreationDate;
                }
                else
                {
                    // Add new
                    context.LearningTrackQuizQuestions.Add(new Entities.LearningTrackQuizQuestion
                    {
                        LearningTrackQuizId = entity.Id,
                        Question = qModel.Question,
                        Answer = qModel.Answer,
                        Explanation = qModel.Explanation,
                        OptionsJson = qModel.OptionsJson,
                        Difficulty = qModel.Difficulty,
                        CreationDate = qModel.CreationDate
                    });
                }
            }

            context.Entry(entity).State = EntityState.Modified;
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated learning track quiz Id={Id}", quiz.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating learning track quiz Id={Id}: {ErrorMessage}", quiz.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteQuizAsync(int id)
        {
            _logger.LogInformation("Deleting learning track quiz Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (entity != null)
            {
                context.LearningTrackQuizzes.Remove(entity);
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted learning track quiz Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting learning track quiz Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Learning track quiz Id={Id} not found for deletion", id);
            }
        }
    }
}