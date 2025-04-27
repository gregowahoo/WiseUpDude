using System.Collections.Generic;
using System.Threading.Tasks;

namespace WiseUpDude.Data.Repositories
{
    public interface IUserRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<Model.Topic>> GetTopicsAsync(int count);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
