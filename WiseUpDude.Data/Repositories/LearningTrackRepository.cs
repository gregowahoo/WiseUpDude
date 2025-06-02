using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackRepository : ILearningTrackRepository
    {
        private readonly ApplicationDbContext _context;
        public LearningTrackRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<LearningTrack>> GetAllAsync() => await _context.LearningTracks.Include(x => x.Categories).ToListAsync();
        public async Task<LearningTrack?> GetByIdAsync(int id) => await _context.LearningTracks.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id);
        public async Task AddAsync(LearningTrack entity) { _context.LearningTracks.Add(entity); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(LearningTrack entity) { _context.LearningTracks.Update(entity); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id) { var entity = await _context.LearningTracks.FindAsync(id); if (entity != null) { _context.LearningTracks.Remove(entity); await _context.SaveChangesAsync(); } }
    }
}
