using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizAttemptRepository : ILearningTrackQuizAttemptRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackQuizAttemptRepository> _logger;

        public LearningTrackQuizAttemptRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackQuizAttemptRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<LearningTrackQuizAttempt?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting quiz attempt by Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackQuizAttempts
                .Include(a => a.AttemptQuestions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<LearningTrackQuizAttempt>> GetAllAsync()
        {
            _logger.LogInformation("Getting all quiz attempts");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackQuizAttempts
                .Include(a => a.AttemptQuestions)
                .ToListAsync();
        }

        public async Task AddAsync(LearningTrackQuizAttempt attempt)
        {
            _logger.LogInformation("Adding quiz attempt for QuizId={QuizId}", attempt.LearningTrackQuizId);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackQuizAttempts.Add(attempt);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully added quiz attempt with Id={Id}", attempt.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding quiz attempt for QuizId={QuizId}: {ErrorMessage}", attempt.LearningTrackQuizId, ex.Message);
                throw;
            }
        }

        public async Task UpdateAsync(LearningTrackQuizAttempt attempt)
        {
            _logger.LogInformation("Updating quiz attempt Id={Id}", attempt.Id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackQuizAttempts.Update(attempt);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated quiz attempt Id={Id}", attempt.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating quiz attempt Id={Id}: {ErrorMessage}", attempt.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting quiz attempt Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizAttempts.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackQuizAttempts.Remove(entity);
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted quiz attempt Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting quiz attempt Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Quiz attempt Id={Id} not found for deletion", id);
            }
        }
    }
}
