using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ILearningTrackQuizRepository
    {
        Task<IEnumerable<LearningTrackQuiz>> GetAllQuizzesAsync();
        Task<LearningTrackQuiz?> GetQuizByIdAsync(int id);
        Task AddQuizAsync(LearningTrackQuiz quiz);
        Task UpdateQuizAsync(LearningTrackQuiz quiz);
        Task DeleteQuizAsync(int id);

        Task<IEnumerable<LearningTrackQuizQuestion>> GetQuestionsByQuizIdAsync(int quizId);
        Task<LearningTrackQuizQuestion?> GetQuestionByIdAsync(int id);
        Task AddQuestionAsync(LearningTrackQuizQuestion question);
        Task UpdateQuestionAsync(LearningTrackQuizQuestion question);
        Task DeleteQuestionAsync(int id);
    }
}
