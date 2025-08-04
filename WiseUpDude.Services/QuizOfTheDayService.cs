using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public interface IQuizOfTheDayService
    {
        Task<Quiz?> GetCurrentQuizOfTheDayAsync();
        Task<IEnumerable<Quiz>> GetAllQuizzesOfTheDayAsync();
        Task SetQuizOfTheDayAsync(int quizId, DateTime? date = null);
        Task RemoveQuizOfTheDayAsync(int quizId);
    }

    public class QuizOfTheDayService : IQuizOfTheDayService
    {
        private readonly IRepository<Quiz> _quizRepository;

        public QuizOfTheDayService(IRepository<Quiz> quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<Quiz?> GetCurrentQuizOfTheDayAsync()
        {
            var today = DateTime.Today;
            var quizzes = await _quizRepository.GetAllAsync();
            return quizzes.FirstOrDefault(q => q.IsQuizOfTheDay && 
                q.QuizOfTheDayDate.HasValue && 
                q.QuizOfTheDayDate.Value.Date == today);
        }

        public async Task<IEnumerable<Quiz>> GetAllQuizzesOfTheDayAsync()
        {
            var quizzes = await _quizRepository.GetAllAsync();
            return quizzes.Where(q => q.IsQuizOfTheDay && q.QuizOfTheDayDate.HasValue)
                         .OrderByDescending(q => q.QuizOfTheDayDate);
        }

        public async Task SetQuizOfTheDayAsync(int quizId, DateTime? date = null)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            quiz.IsQuizOfTheDay = true;
            quiz.QuizOfTheDayDate = date ?? DateTime.Today;
            await _quizRepository.UpdateAsync(quiz);
        }

        public async Task RemoveQuizOfTheDayAsync(int quizId)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            quiz.IsQuizOfTheDay = false;
            quiz.QuizOfTheDayDate = null;
            await _quizRepository.UpdateAsync(quiz);
        }
    }
}