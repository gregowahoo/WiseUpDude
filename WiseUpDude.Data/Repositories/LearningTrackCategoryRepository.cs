using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackCategoryRepository : ILearningTrackCategoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackCategoryRepository> _logger;

        public LearningTrackCategoryRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackCategoryRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Model.LearningTrackCategory>> GetAllAsync()
        {
            _logger.LogInformation("Getting all learning track categories");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.LearningTrackCategories
                                        .Include(x => x.Sources)
                                        .ToListAsync();
            return entities.Select(entity => EntityToModel(entity));
        }

        public async Task<Model.LearningTrackCategory?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting learning track category by Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackCategories
                                      .Include(x => x.Sources)
                                      .FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrackCategory model)
        {
            _logger.LogInformation("Adding learning track category: {@Model}", model);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            context.LearningTrackCategories.Add(entity);
            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully added learning track category with Id={Id}", entity.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding learning track category: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task UpdateAsync(Model.LearningTrackCategory model)
        {
            _logger.LogInformation("Updating learning track category Id={Id}", model.Id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackCategories.FindAsync(model.Id);
            if (entity != null)
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.Difficulty = model.Difficulty;
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated learning track category Id={Id}", model.Id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error updating learning track category Id={Id}: {ErrorMessage}", model.Id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Learning track category Id={Id} not found for update", model.Id);
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting learning track category Id={Id}", id);
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackCategories.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackCategories.Remove(entity);
                try
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted learning track category Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting learning track category Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Learning track category Id={Id} not found for deletion", id);
            }
        }

        private Model.LearningTrackCategory EntityToModel(Entities.LearningTrackCategory entity)
        {
            if (entity == null) return null;
            return new Model.LearningTrackCategory
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Difficulty = entity.Difficulty,
                LearningTrackId = entity.LearningTrackId,
                CreationDate = entity.CreationDate,
                Sources = entity.Sources?.Select(s => new Model.LearningTrackSource { Id = s.Id, Name = s.Name, Url = s.Url, Description = s.Description, SourceType = s.SourceType, LearningTrackCategoryId = s.LearningTrackCategoryId }).ToList()
            };
        }

        private Entities.LearningTrackCategory ModelToEntity(Model.LearningTrackCategory model)
        {
            if (model == null) return null;
            return new Entities.LearningTrackCategory
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Difficulty = model.Difficulty,
                LearningTrackId = model.LearningTrackId,
                CreationDate = model.CreationDate == DateTime.MinValue ? DateTime.UtcNow : model.CreationDate
            };
        }
    }
}