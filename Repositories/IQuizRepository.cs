using WiseUpDude.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WiseUpDude.Repositories
{
    public interface IQuizRepository
    {
        Task<Quiz> GetQuizByIdAsync(int id);
        Task<IEnumerable<Quiz>> GetAllQuizzesAsync();
        Task AddQuizAsync(Quiz quiz);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(int id);
    }
}
