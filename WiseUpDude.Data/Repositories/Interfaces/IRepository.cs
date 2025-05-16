using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);

        // Add this method for fetching quizzes by TopicId
        Task<IEnumerable<Quiz>> GetQuizzesByTopicIdAsync(int topicId);
    }
}
