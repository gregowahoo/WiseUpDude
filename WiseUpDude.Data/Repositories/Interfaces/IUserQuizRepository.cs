using System.Collections.Generic;
using System.Threading.Tasks;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface IUserQuizRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task UpdateQuizNameAsync(int id, string newName);
        
        // New method to update LearnMode
        Task UpdateLearnModeAsync(int id, bool learnMode);
    }
}
