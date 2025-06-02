using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackSourceRepository : ILearningTrackSourceRepository
    {
        private readonly ApplicationDbContext _context;
        public LearningTrackSourceRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<LearningTrackSource>> GetAllAsync() => await _context.LearningTrackSources.Include(x => x.Quizzes).ToListAsync();
        public async Task<LearningTrackSource?> GetByIdAsync(int id) => await _context.LearningTrackSources.Include(x => x.Quizzes).FirstOrDefaultAsync(x => x.Id == id);
        public async Task AddAsync(LearningTrackSource entity) { _context.LearningTrackSources.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(LearningTrackSource entity) { _context.LearningTrackSources.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var entity = await _context.LearningTrackSources.FindAsync(id); if (entity != null) { _context.LearningTrackSources.Remove(entity); await _context.SaveChangesAsync(); } }
    }
}
