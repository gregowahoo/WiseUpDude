using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizAttemptQuestionRepository<T> where T : class
    {
        Task<UserQuizAttemptQuestion?> GetByIdAsync(int id);
        Task<IEnumerable<UserQuizAttemptQuestion>> GetByAttemptIdAsync(int attemptId);
        Task AddAsync(UserQuizAttemptQuestion question);
        Task UpdateAsync(UserQuizAttemptQuestion question);
        Task DeleteAsync(int id);
    }
}