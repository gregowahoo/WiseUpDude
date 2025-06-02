using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackSourceRepository
    {
        Task<IEnumerable<Model.LearningTrackSource>> GetAllAsync();
        Task<Model.LearningTrackSource?> GetByIdAsync(int id);
        Task AddAsync(Model.LearningTrackSource entity);
        Task UpdateAsync(Model.LearningTrackSource entity);
        Task DeleteAsync(int id);
    }
}
