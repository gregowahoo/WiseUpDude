// File: WiseUpDude.Shared\Services\IUserQuizAttemptApiService.cs
using WiseUpDude.Model;

namespace WiseUpDude.Shared.Services
{
    public interface IUserQuizAttemptApiService
    {
        Task<UserQuizAttempt?> GetByIdAsync(int id);
        Task<List<UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId);
        Task<UserQuizAttempt?> CreateAsync(UserQuizAttempt attempt);
        Task<bool> UpdateAsync(UserQuizAttempt attempt);
        Task<bool> DeleteAsync(int id);
    }
}