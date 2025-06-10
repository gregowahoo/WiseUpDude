using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackQuizAttemptQuestionRepository
    {
        Task<LearningTrackQuizAttemptQuestion?> GetByIdAsync(int id);
        Task<IEnumerable<LearningTrackQuizAttemptQuestion>> GetAllAsync();
        Task AddAsync(LearningTrackQuizAttemptQuestion question);
        Task UpdateAsync(LearningTrackQuizAttemptQuestion question);
        Task DeleteAsync(int id);
    }
}
