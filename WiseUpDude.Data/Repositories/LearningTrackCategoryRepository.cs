using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackCategoryRepository : ILearningTrackCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public LearningTrackCategoryRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Model.LearningTrackCategory>> GetAllAsync()
        {
            var entities = await _context.LearningTrackCategories.Include(x => x.Sources).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrackCategory?> GetByIdAsync(int id)
        {
            var entity = await _context.LearningTrackCategories.Include(x => x.Sources).FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrackCategory model)
        {
            var entity = ModelToEntity(model);
            _context.LearningTrackCategories.Add(entity);
            await _context.SaveChangesAsync();
            model.Id = entity.Id;
        }

        public async Task UpdateAsync(Model.LearningTrackCategory model)
        {
            var entity = await _context.LearningTrackCategories.Include(x => x.Sources).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (entity == null) return;
            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Difficulty = model.Difficulty;
            // Update sources if needed
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.LearningTrackCategories.FindAsync(id);
            if (entity != null)
            {
                _context.LearningTrackCategories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // --- Mapping helpers ---
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

        private static LearningTrackCategory ModelToEntity(Model.LearningTrackCategory model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Difficulty = model.Difficulty,
            LearningTrackId = model.LearningTrackId,
            CreationDate = model.CreationDate,
            Sources = model.Sources?.Select(ModelToEntity).ToList() ?? new()
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