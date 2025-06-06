using Microsoft.EntityFrameworkCore;
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

        public LearningTrackCategoryRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<Model.LearningTrackCategory>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var entities = await context.LearningTrackCategories
                                        .Include(x => x.Sources) // If you need to include related entities
                                        .ToListAsync();

            // Assuming you have an EntityToModel mapping method or similar
            // If not, you'd map properties manually or use a library like AutoMapper
            return entities.Select(entity => EntityToModel(entity));
        }

        public async Task<Model.LearningTrackCategory?> GetByIdAsync(int id) // Assuming your interface returns Model.LearningTrackCategory?
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackCategories
                                      .Include(x => x.Sources) // Example include
                                      .FirstOrDefaultAsync(x => x.Id == id);
            return entity == null ? null : EntityToModel(entity);
        }

        public async Task AddAsync(Model.LearningTrackCategory model) // Assuming your interface takes Model.LearningTrackCategory
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = ModelToEntity(model); // You'll need a method to map from Model to Entity
            context.LearningTrackCategories.Add(entity);
            await context.SaveChangesAsync();
            // If you need to update the model with the new ID:
            // model.Id = entity.Id; // EF Core populates the ID on the tracked entity after SaveChangesAsync
        }


        public async Task UpdateAsync(Model.LearningTrackCategory model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            // You might need to fetch the existing entity first to ensure it's tracked
            var entity = await context.LearningTrackCategories.FindAsync(model.Id);
            if (entity != null)
            {
                // Update properties from model to entity
                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.Difficulty = model.Difficulty;
                // Update other properties as needed, but be careful with navigation properties
                // context.Entry(entity).State = EntityState.Modified; // Not always needed if you modify properties on a tracked entity
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.LearningTrackCategories.FindAsync(id);
            if (entity != null)
            {
                context.LearningTrackCategories.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        // Placeholder for your mapping logic (Model = DTO/ViewModel, Entity = EF Core DB class)
        // You would implement these based on your specific Model and Entity structures.
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
                // Map Sources if needed
                Sources = entity.Sources?.Select(s => new Model.LearningTrackSource { /* map source properties */ Id = s.Id, Name = s.Name, Url = s.Url, Description = s.Description, SourceType = s.SourceType, LearningTrackCategoryId = s.LearningTrackCategoryId }).ToList()
            };
        }

        private Entities.LearningTrackCategory ModelToEntity(Model.LearningTrackCategory model)
        {
            if (model == null) return null; // Or throw argument null exception
            // When creating a new entity from a model for an Add operation,
            // you generally don't set the Id if it's database-generated.
            return new Entities.LearningTrackCategory
            {
                Id = model.Id, // For updates, Id is important. For adds, it might be 0.
                Name = model.Name,
                Description = model.Description,
                Difficulty = model.Difficulty,
                LearningTrackId = model.LearningTrackId,
                CreationDate = model.CreationDate == DateTime.MinValue ? DateTime.UtcNow : model.CreationDate // Handle CreationDate
                // Do NOT map navigation properties like 'Sources' here if you're just adding/updating the category itself.
                // Managing relationships is a separate concern.
            };
        }
    }
}