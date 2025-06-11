using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizAttemptRepository : ILearningTrackQuizAttemptRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public LearningTrackQuizAttemptRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<LearningTrackQuizAttempt?> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackQuizAttempts
                .Include(a => a.AttemptQuestions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<LearningTrackQuizAttempt>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackQuizAttempts
                .Include(a => a.AttemptQuestions)
                .ToListAsync();
        }

        public async Task AddAsync(LearningTrackQuizAttempt attempt)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackQuizAttempts.Add(attempt);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LearningTrackQuizAttempt attempt)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackQuizAttempts.Update(attempt);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizAttempts.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackQuizAttempts.Remove(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}
