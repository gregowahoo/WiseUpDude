using WiseUpDude.Data.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizAttemptQuestionRepository
    {
        Task<UserQuizAttemptQuestion?> GetByIdAsync(int id);
        Task<IEnumerable<UserQuizAttemptQuestion>> GetByAttemptIdAsync(int attemptId);
        Task AddAsync(UserQuizAttemptQuestion question);
        Task UpdateAsync(UserQuizAttemptQuestion question);
        Task DeleteAsync(int id);
    }
}