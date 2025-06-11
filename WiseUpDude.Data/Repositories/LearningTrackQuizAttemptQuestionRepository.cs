using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizAttemptQuestionRepository : ILearningTrackQuizAttemptQuestionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public LearningTrackQuizAttemptQuestionRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<LearningTrackQuizAttemptQuestion?> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackAttemptQuestions
                .Include(q => q.LearningTrackAttempt)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<LearningTrackQuizAttemptQuestion>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.LearningTrackAttemptQuestions
                .Include(q => q.LearningTrackAttempt)
                .ToListAsync();
        }

        public async Task AddAsync(LearningTrackQuizAttemptQuestion question)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackAttemptQuestions.Add(question);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LearningTrackQuizAttemptQuestion question)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.LearningTrackAttemptQuestions.Update(question);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackAttemptQuestions.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackAttemptQuestions.Remove(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}
