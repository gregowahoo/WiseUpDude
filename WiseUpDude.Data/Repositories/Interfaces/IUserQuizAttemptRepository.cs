using WiseUpDude.Data.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizAttemptRepository
    {
        Task<UserQuizAttempt?> GetByIdAsync(int id);
        Task<IEnumerable<UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId);
        Task AddAsync(UserQuizAttempt attempt);
        Task UpdateAsync(UserQuizAttempt attempt);
        Task DeleteAsync(int id);
    }
}