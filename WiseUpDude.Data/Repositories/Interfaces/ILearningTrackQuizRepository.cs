using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using Model = WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackQuizRepository
    {
        Task<IEnumerable<Model.LearningTrackQuiz>> GetAllQuizzesAsync();
        Task<Model.LearningTrackQuiz?> GetQuizByIdAsync(int id);
        Task<int> AddQuizAsync(Model.LearningTrackQuiz quiz);
        Task UpdateQuizAsync(Model.LearningTrackQuiz quiz);
        Task DeleteQuizAsync(int id);
    }
}
