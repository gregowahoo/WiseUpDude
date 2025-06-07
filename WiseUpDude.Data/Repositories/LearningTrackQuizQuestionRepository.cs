using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizQuestionRepository : ILearningTrackQuizQuestionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public LearningTrackQuizQuestionRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

        public async Task<IEnumerable<Model.LearningTrackQuizQuestion>> GetQuestionsByQuizIdAsync(int quizId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTrackQuizQuestions.Where(q => q.LearningTrackQuizId == quizId).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrackQuizQuestion?> GetQuestionByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddQuestionAsync(Model.LearningTrackQuizQuestion model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            context.LearningTrackQuizQuestions.Add(entity);
            await context.SaveChangesAsync();
            model.Id = entity.Id;
        }

        public async Task UpdateQuestionAsync(Model.LearningTrackQuizQuestion model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizQuestions.FirstOrDefaultAsync(q => q.Id == model.Id);
            if (entity == null) return;
            entity.Question = model.Question;
            entity.Answer = model.Answer;
            entity.Explanation = model.Explanation;
            entity.OptionsJson = model.OptionsJson;
            entity.Difficulty = model.Difficulty;
            await context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizQuestions.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackQuizQuestions.Remove(entity);
                await context.SaveChangesAsync();
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
