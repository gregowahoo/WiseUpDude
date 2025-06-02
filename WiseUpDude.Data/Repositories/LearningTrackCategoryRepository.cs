using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackCategoryRepository : ILearningTrackCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public LearningTrackCategoryRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<LearningTrackCategory>> GetAllAsync() => await _context.LearningTrackCategories.Include(x => x.Sources).ToListAsync();
        public async Task<LearningTrackCategory?> GetByIdAsync(int id) => await _context.LearningTrackCategories.Include(x => x.Sources).FirstOrDefaultAsync(x => x.Id == id);
        public async Task AddAsync(LearningTrackCategory entity) { _context.LearningTrackCategories.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(LearningTrackCategory entity) { _context.LearningTrackCategories.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var entity = await _context.LearningTrackCategories.FindAsync(id); if (entity != null) { _context.LearningTrackCategories.Remove(entity); await _context.SaveChangesAsync(); } }
    }
}
