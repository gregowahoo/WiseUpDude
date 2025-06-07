using System.Collections.Generic;
using System.Threading.Tasks;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackQuizQuestionRepository
    {
        Task<IEnumerable<Model.LearningTrackQuizQuestion>> GetQuestionsByQuizIdAsync(int quizId);
        Task<Model.LearningTrackQuizQuestion?> GetQuestionByIdAsync(int id);
        Task AddQuestionAsync(Model.LearningTrackQuizQuestion question);
        Task UpdateQuestionAsync(Model.LearningTrackQuizQuestion question);
        Task DeleteQuestionAsync(int id);
    }
}
