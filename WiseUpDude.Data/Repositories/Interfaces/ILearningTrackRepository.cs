using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackRepository
    {
        Task<IEnumerable<LearningTrack>> GetAllAsync();
        Task<LearningTrack?> GetByIdAsync(int id);
        Task AddAsync(LearningTrack entity);
        Task UpdateAsync(LearningTrack entity);
        Task DeleteAsync(int id);
    }
}
