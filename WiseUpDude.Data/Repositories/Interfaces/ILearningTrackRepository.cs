using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackRepository
    {
        Task<IEnumerable<Model.LearningTrack>> GetAllAsync();
        Task<Model.LearningTrack?> GetByIdAsync(int id);
        Task AddAsync(Model.LearningTrack model);
        Task UpdateAsync(Model.LearningTrack model);
        Task DeleteAsync(int id);
    }
}
