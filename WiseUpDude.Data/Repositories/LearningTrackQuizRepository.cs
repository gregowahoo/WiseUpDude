using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizRepository : ILearningTrackQuizRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public LearningTrackQuizRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

        public async Task<IEnumerable<Model.LearningTrackQuiz>> GetAllQuizzesAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTrackQuizzes.Include(q => q.Questions).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrackQuiz?> GetQuizByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddQuizAsync(Model.LearningTrackQuiz model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            context.LearningTrackQuizzes.Add(entity);
            await context.SaveChangesAsync();
            model.Id = entity.Id;
        }

        public async Task UpdateQuizAsync(Model.LearningTrackQuiz model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == model.Id);
            if (entity == null) return;
            entity.Name = model.Name;
            entity.Description = model.Description;
            // Update other fields as needed
            await context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackQuizzes.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackQuizzes.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

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

        // --- Mapping helpers ---
        private static Model.LearningTrackQuiz EntityToModel(LearningTrackQuiz entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            LearningTrackSourceId = entity.LearningTrackSourceId,
            CreationDate = entity.CreationDate,
            Questions = entity.Questions?.Select(EntityToModel).ToList() ?? new()
        };

        private static Model.LearningTrackQuizQuestion EntityToModel(LearningTrackQuizQuestion entity) => new()
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

        private static LearningTrackQuiz ModelToEntity(Model.LearningTrackQuiz model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            LearningTrackSourceId = model.LearningTrackSourceId,
            CreationDate = model.CreationDate,
            Questions = model.Questions?.Select(ModelToEntity).ToList() ?? new()
        };

        private static LearningTrackQuizQuestion ModelToEntity(Model.LearningTrackQuizQuestion model) => new()
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