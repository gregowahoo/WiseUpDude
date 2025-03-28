using WiseUpDude.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WiseUpDude.Repositories
{
    public interface IQuizQuestionRepository
    {
        Task<QuizQuestion> GetQuizQuestionByIdAsync(int id);
        Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync();
        Task AddQuizQuestionAsync(QuizQuestion quizQuestion);
        Task UpdateQuizQuestionAsync(QuizQuestion quizQuestion);
        Task DeleteQuizQuestionAsync(int id);
    }
}
