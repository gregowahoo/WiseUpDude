using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ITopicRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Topic>> GetTopicsAsync(int count);
        Task<IEnumerable<T>> GetTopicsByCategoryAsync(int categoryId); // New method
    }
}