using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackRepository : ILearningTrackRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        public LearningTrackRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

        public async Task<IEnumerable<Model.LearningTrack>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTracks.Include(x => x.Categories).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrack?> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrack model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            context.LearningTracks.Add(entity);
            await context.SaveChangesAsync();
            model.Id = entity.Id;
        }

        public async Task UpdateAsync(Model.LearningTrack model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null) return;
            entity.Name = model.Name;
            entity.Description = model.Description;
            // update other fields as needed
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTracks.FindAsync(id);
            if (entity != null)
            {
                context.LearningTracks.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        // --- Mapping helpers ---
        private static Model.LearningTrack EntityToModel(LearningTrack entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            UserId = entity.UserId,
            CreationDate = entity.CreationDate,
            Categories = entity.Categories?.Select(EntityToModel).ToList() ?? new()
        };

        private static Model.LearningTrackCategory EntityToModel(LearningTrackCategory entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Difficulty = entity.Difficulty,
            LearningTrackId = entity.LearningTrackId,
            CreationDate = entity.CreationDate,
            Sources = entity.Sources?.Select(EntityToModel).ToList() ?? new()
        };

        private static Model.LearningTrackSource EntityToModel(LearningTrackSource entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            SourceType = entity.SourceType,
            Url = entity.Url,
            Description = entity.Description,
            LearningTrackCategoryId = entity.LearningTrackCategoryId,
            CreationDate = entity.CreationDate
            // Quizzes mapping if needed
        };

        private static LearningTrack ModelToEntity(Model.LearningTrack model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            UserId = model.UserId,
            Categories = model.Categories?.Select(ModelToEntity).ToList() ?? new()
        };

        private static LearningTrackCategory ModelToEntity(Model.LearningTrackCategory model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Difficulty = model.Difficulty,
            LearningTrackId = model.LearningTrackId,
            Sources = model.Sources?.Select(ModelToEntity).ToList() ?? new()
        };

        private static LearningTrackSource ModelToEntity(Model.LearningTrackSource model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            SourceType = model.SourceType,
            Url = model.Url,
            Description = model.Description,
            LearningTrackCategoryId = model.LearningTrackCategoryId
            // Quizzes mapping if needed
        };
    }
}