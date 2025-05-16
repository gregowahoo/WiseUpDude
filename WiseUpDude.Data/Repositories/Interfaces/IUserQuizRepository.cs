using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task<int> AddAsyncGetId(Model.Quiz quiz);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<List<Model.Quiz>> GetUserQuizzesAsync(string userId);
        Task UpdateQuizNameAsync(int id, string newName);
        Task<List<RecentQuizDto>> GetRecentUserQuizzesAsync(string userId, int count = 5);

        // New method to update LearnMode
        Task UpdateLearnModeAsync(int id, bool learnMode);
    }
}
