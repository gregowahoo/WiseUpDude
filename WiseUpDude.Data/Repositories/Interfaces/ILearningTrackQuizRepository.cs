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
        Task AddQuizAsync(Model.LearningTrackQuiz quiz);
        Task UpdateQuizAsync(Model.LearningTrackQuiz quiz);
        Task DeleteQuizAsync(int id);

        Task<IEnumerable<Model.LearningTrackQuizQuestion>> GetQuestionsByQuizIdAsync(int quizId);
        Task<Model.LearningTrackQuizQuestion?> GetQuestionByIdAsync(int id);
        Task AddQuestionAsync(Model.LearningTrackQuizQuestion question);
        Task UpdateQuestionAsync(Model.LearningTrackQuizQuestion question);
        Task DeleteQuestionAsync(int id);
    }
}
