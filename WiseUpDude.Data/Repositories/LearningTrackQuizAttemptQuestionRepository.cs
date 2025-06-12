using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizAttemptQuestionRepository : ILearningTrackQuizAttemptQuestionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackQuizAttemptQuestionRepository> _logger;

        public LearningTrackQuizAttemptQuestionRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackQuizAttemptQuestionRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<LearningTrackQuizAttemptQuestion?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting quiz attempt question by Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackAttemptQuestions
                .Include(q => q.LearningTrackAttempt)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<LearningTrackQuizAttemptQuestion>> GetAllAsync()
        {
            _logger.LogInformation("Getting all quiz attempt questions");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackAttemptQuestions
                .Include(q => q.LearningTrackAttempt)
                .ToListAsync();
        }

        public async Task AddAsync(LearningTrackQuizAttemptQuestion question)
        {
            _logger.LogInformation("Adding quiz attempt question for AttemptId={AttemptId}, QuestionId={QuestionId}", question.LearningTrackAttemptId, question.LearningTrackQuestionId);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackAttemptQuestions.Add(question);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully added quiz attempt question with Id={Id}", question.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding quiz attempt question for AttemptId={AttemptId}, QuestionId={QuestionId}: {ErrorMessage}", question.LearningTrackAttemptId, question.LearningTrackQuestionId, ex.Message);
                throw;
            }
        }

        public async Task UpdateAsync(LearningTrackQuizAttemptQuestion question)
        {
            _logger.LogInformation("Updating quiz attempt question Id={Id}", question.Id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackAttemptQuestions.Update(question);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated quiz attempt question Id={Id}", question.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating quiz attempt question Id={Id}: {ErrorMessage}", question.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting quiz attempt question Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackAttemptQuestions.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackAttemptQuestions.Remove(entity);
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted quiz attempt question Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting quiz attempt question Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Quiz attempt question Id={Id} not found for deletion", id);
            }
        }
    }
}
