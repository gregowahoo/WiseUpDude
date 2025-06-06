using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackSourceRepository : ILearningTrackSourceRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public LearningTrackSourceRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<Model.LearningTrackSource>> GetAllAsync()
        {
            await using var _context = await _dbContextFactory.CreateDbContextAsync();

            var entities = await _context.LearningTrackSources.Include(x => x.Quizzes).ToListAsync();
            return entities.Select(EntityToModel);
        }

        public async Task<Model.LearningTrackSource?> GetByIdAsync(int id)
        {
            await using var _context = await _dbContextFactory.CreateDbContextAsync();

            var entity = await _context.LearningTrackSources.Include(x => x.Quizzes).FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrackSource model)
        {
            await using var _context = await _dbContextFactory.CreateDbContextAsync();

            var entity = ModelToEntity(model);

            _context.LearningTrackSources.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
        }

        public async Task UpdateAsync(Model.LearningTrackSource model)
        {
            await using var _context = await _dbContextFactory.CreateDbContextAsync();

            var entity = await _context.LearningTrackSources.Include(x => x.Quizzes).FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity == null) return;
            entity.Name = model.Name;
            entity.SourceType = model.SourceType;
            entity.Url = model.Url;
            entity.Description = model.Description;
            // Update quizzes if needed
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var _context = await _dbContextFactory.CreateDbContextAsync();

            var entity = await _context.LearningTrackSources.FindAsync(id);
            if (entity != null)
            {
                _context.LearningTrackSources.Remove(entity);
                await _context.SaveChangesAsync();
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
            // Quizzes mapping if needed
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
            // Quizzes mapping if needed
        };
    }
}