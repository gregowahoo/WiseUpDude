using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizAttemptRepository<T> where T : class
    {
        Task<UserQuizAttempt?> GetByIdAsync(int id);
        Task<IEnumerable<UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId);
        Task<Model.UserQuizAttempt> AddAsync(Model.UserQuizAttempt attempt);
        Task UpdateAsync(UserQuizAttempt attempt);
        Task DeleteAsync(int id);
    }
}