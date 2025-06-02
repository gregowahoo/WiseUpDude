using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackSourceRepository
    {
        Task<IEnumerable<LearningTrackSource>> GetAllAsync();
        Task<LearningTrackSource?> GetByIdAsync(int id);
        Task AddAsync(LearningTrackSource entity);
        Task UpdateAsync(LearningTrackSource entity);
        Task DeleteAsync(int id);
    }
}
