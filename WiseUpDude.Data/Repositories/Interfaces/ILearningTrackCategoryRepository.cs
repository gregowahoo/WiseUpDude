using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackCategoryRepository
    {
        Task<IEnumerable<Model.LearningTrackCategory>> GetAllAsync();
        Task<Model.LearningTrackCategory?> GetByIdAsync(int id);
        Task AddAsync(Model.LearningTrackCategory entity);
        Task UpdateAsync(Model.LearningTrackCategory entity);
        Task DeleteAsync(int id);
    }
}
