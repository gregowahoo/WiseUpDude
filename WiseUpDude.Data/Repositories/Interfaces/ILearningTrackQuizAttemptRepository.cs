using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackQuizAttemptRepository
    {
        Task<LearningTrackQuizAttempt?> GetByIdAsync(int id);
        Task<IEnumerable<LearningTrackQuizAttempt>> GetAllAsync();
        Task AddAsync(LearningTrackQuizAttempt attempt);
        Task UpdateAsync(LearningTrackQuizAttempt attempt);
        Task DeleteAsync(int id);
    }
}
