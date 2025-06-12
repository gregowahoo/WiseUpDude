using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackRepository : ILearningTrackRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackRepository> _logger;

        public LearningTrackRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Model.LearningTrack>> GetAllAsync()
        {
            _logger.LogInformation("Getting all learning tracks");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTracks.Include(x => x.Categories).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrack?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting learning track by Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrack model)
        {
            _logger.LogInformation("Adding learning track: {@Model}", model);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            context.LearningTracks.Add(entity);
            try
            {
                await context.SaveChangesAsync();
                model.Id = entity.Id;
                _logger.LogInformation("Successfully added learning track with Id={Id}", entity.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding learning track: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task UpdateAsync(Model.LearningTrack model)
        {
            _logger.LogInformation("Updating learning track Id={Id}", model.Id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null)
            {
                _logger.LogWarning("Learning track Id={Id} not found for update", model.Id);
                return;
            }
            entity.Name = model.Name;
            entity.Description = model.Description;
            // update other fields as needed
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated learning track Id={Id}", model.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating learning track Id={Id}: {ErrorMessage}", model.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting learning track Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTracks.FindAsync(id);
            if (entity != null)
            {
                context.LearningTracks.Remove(entity);
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted learning track Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting learning track Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Learning track Id={Id} not found for deletion", id);
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
        };
    }
}