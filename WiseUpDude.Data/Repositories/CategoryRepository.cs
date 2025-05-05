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
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WiseUpDude.Model.Category>> GetAllAsync()
        {
            var entities = await _context.Categories
                .Include(c => c.Topics) // Include related Topics if needed
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
            var entity = await _context.Categories
                .Include(c => c.Topics) // Include related Topics if needed
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
            var entity = new Entities.Category
            {
                Name = category.Name,
                Description = category.Description
            };

            await _context.Categories.AddAsync(entity);
            await _context.SaveChangesAsync();

            category.Id = entity.Id;
        }

        public async Task UpdateAsync(WiseUpDude.Model.Category category)
        {
            var entity = await _context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

            if (entity == null)
                throw new KeyNotFoundException($"Category with Id {category.Id} not found.");

            entity.Name = category.Name;
            entity.Description = category.Description;

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity != null)
            {
                _context.Categories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
