using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizQuestionRepository : ILearningTrackQuizQuestionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackQuizQuestionRepository> _logger;
        public LearningTrackQuizQuestionRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackQuizQuestionRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Model.LearningTrackQuizQuestion>> GetQuestionsByQuizIdAsync(int quizId)
        {
            _logger.LogInformation("Getting questions for QuizId={QuizId}", quizId);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTrackQuizQuestions.Where(q => q.LearningTrackQuizId == quizId).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrackQuizQuestion?> GetQuestionByIdAsync(int id)
        {
            _logger.LogInformation("Getting question by Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddQuestionAsync(Model.LearningTrackQuizQuestion model)
        {
            _logger.LogInformation("Adding question to QuizId={QuizId}: {@Model}", model.LearningTrackQuizId, model);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            context.LearningTrackQuizQuestions.Add(entity);
            try
            {
                await context.SaveChangesAsync();
                model.Id = entity.Id;
                _logger.LogInformation("Successfully added question with Id={Id} to QuizId={QuizId}", entity.Id, entity.LearningTrackQuizId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding question to QuizId={QuizId}: {ErrorMessage}", model.LearningTrackQuizId, ex.Message);
                throw;
            }
        }

        public async Task UpdateQuestionAsync(Model.LearningTrackQuizQuestion model)
        {
            _logger.LogInformation("Updating question Id={Id}", model.Id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizQuestions.FirstOrDefaultAsync(q => q.Id == model.Id);
            if (entity == null)
            {
                _logger.LogWarning("Question Id={Id} not found for update", model.Id);
                return;
            }
            entity.Question = model.Question;
            entity.Answer = model.Answer;
            entity.Explanation = model.Explanation;
            entity.OptionsJson = model.OptionsJson;
            entity.Difficulty = model.Difficulty;
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated question Id={Id}", model.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating question Id={Id}: {ErrorMessage}", model.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteQuestionAsync(int id)
        {
            _logger.LogInformation("Deleting question Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizQuestions.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackQuizQuestions.Remove(entity);
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted question Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting question Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Question Id={Id} not found for deletion", id);
            }
        }

        private static Model.LearningTrackQuizQuestion EntityToModel(Data.Entities.LearningTrackQuizQuestion entity) => new()
        {
            Id = entity.Id,
            LearningTrackQuizId = entity.LearningTrackQuizId,
            Question = entity.Question,
            Answer = entity.Answer,
            Explanation = entity.Explanation,
            OptionsJson = entity.OptionsJson,
            Difficulty = entity.Difficulty,
            CreationDate = entity.CreationDate
        };

        private static Data.Entities.LearningTrackQuizQuestion ModelToEntity(Model.LearningTrackQuizQuestion model) => new()
        {
            Id = model.Id,
            LearningTrackQuizId = model.LearningTrackQuizId,
            Question = model.Question,
            Answer = model.Answer,
            Explanation = model.Explanation,
            OptionsJson = model.OptionsJson,
            Difficulty = model.Difficulty,
            CreationDate = model.CreationDate
        };
    }
}
