using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackCategoryRepository
    {
        Task<IEnumerable<LearningTrackCategory>> GetAllAsync();
        Task<LearningTrackCategory?> GetByIdAsync(int id);
        Task AddAsync(LearningTrackCategory entity);
        Task UpdateAsync(LearningTrackCategory entity);
        Task DeleteAsync(int id);
    }
}
