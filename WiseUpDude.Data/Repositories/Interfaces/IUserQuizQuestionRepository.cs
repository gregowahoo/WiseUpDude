using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizQuestionRepository<T> where T : class
    {
        Task AddAsync(QuizQuestion quizQuestion);
        Task DeleteAsync(int id);
        Task<IEnumerable<QuizQuestion>> GetAllAsync();
        Task<QuizQuestion> GetByIdAsync(int id);
        Task UpdateAsync(QuizQuestion quizQuestion);
    }
}