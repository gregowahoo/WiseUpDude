using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public CategoryRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<WiseUpDude.Model.Category>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.Categories
                .Include(c => c.Topics)
                .ToListAsync();
            return entities.Select(e => new WiseUpDude.Model.Category
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            });
        }

        public async Task<WiseUpDude.Model.Category?> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Categories
                .Include(c => c.Topics)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
                return null;
            return new WiseUpDude.Model.Category
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public async Task AddAsync(WiseUpDude.Model.Category category)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = new Entities.Category
            {
                Name = category.Name,
                Description = category.Description
            };
            await context.Categories.AddAsync(entity);
            await context.SaveChangesAsync();
            category.Id = entity.Id;
        }

        public async Task UpdateAsync(WiseUpDude.Model.Category category)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Category with Id {category.Id} not found.");
            entity.Name = category.Name;
            entity.Description = category.Description;
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.Categories.FindAsync(id);
            if (entity != null)
            {
                context.Categories.Remove(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}
