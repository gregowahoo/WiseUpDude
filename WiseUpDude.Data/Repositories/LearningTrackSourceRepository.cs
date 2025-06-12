using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackSourceRepository : ILearningTrackSourceRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<LearningTrackSourceRepository> _logger;

        public LearningTrackSourceRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<LearningTrackSourceRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<Model.LearningTrackSource>> GetAllAsync()
        {
            _logger.LogInformation("Getting all learning track sources");
            await using var _context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await _context.LearningTrackSources.Include(x => x.Quizzes).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrackSource?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting learning track source by Id={Id}", id);
            await using var _context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await _context.LearningTrackSources.Include(x => x.Quizzes).FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrackSource model)
        {
            _logger.LogInformation("Adding learning track source: {@Model}", model);
            await using var _context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model);
            _context.LearningTrackSources.Add(entity);
            try
            {
                await _context.SaveChangesAsync();
                model.Id = entity.Id;
                _logger.LogInformation("Successfully added learning track source with Id={Id}", entity.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding learning track source: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task UpdateAsync(Model.LearningTrackSource model)
        {
            _logger.LogInformation("Updating learning track source Id={Id}", model.Id);
            await using var _context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await _context.LearningTrackSources.Include(x => x.Quizzes).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null)
            {
                _logger.LogWarning("Learning track source Id={Id} not found for update", model.Id);
                return;
            }
            entity.Name = model.Name;
            entity.SourceType = model.SourceType;
            entity.Url = model.Url;
            entity.Description = model.Description;
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated learning track source Id={Id}", model.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating learning track source Id={Id}: {ErrorMessage}", model.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting learning track source Id={Id}", id);
            await using var _context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await _context.LearningTrackSources.FindAsync(id);
            if (entity != null)
            {
                _context.LearningTrackSources.Remove(entity);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully deleted learning track source Id={Id}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error deleting learning track source Id={Id}: {ErrorMessage}", id, ex.Message);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Learning track source Id={Id} not found for deletion", id);
            }
        }

        // --- Mapping helpers ---
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

        private static LearningTrackSource ModelToEntity(Model.LearningTrackSource model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            SourceType = model.SourceType,
            Url = model.Url,
            Description = model.Description,
            LearningTrackCategoryId = model.LearningTrackCategoryId,
            CreationDate = model.CreationDate
        };
    }
}