using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IQuizQuestionRepository<T>
    {
        Task AddAsync(QuizQuestion model);
        Task DeleteAsync(int id);
        Task<IEnumerable<QuizQuestion>> GetAllAsync();
        Task<QuizQuestion> GetByIdAsync(int id);
        Task UpdateAsync(QuizQuestion model);
    }
}