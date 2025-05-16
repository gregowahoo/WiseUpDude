using WiseUpDude.Services;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Services
{
    public class QuizStateService
    {
        private readonly IUserQuizRepository<Quiz> _quizRepository;

        public Quiz? CurrentQuiz { get; set; }

        public event Action? OnQuizUpdated;

        public QuizStateService(IUserQuizRepository<Quiz> quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<Quiz?> LoadQuizFromDatabaseAsync(int quizId)
        {
            CurrentQuiz = await _quizRepository.GetByIdAsync(quizId);
            OnQuizUpdated?.Invoke(); // Notify subscribers
            return CurrentQuiz;
        }
        public void UpdateCurrentQuiz(Quiz quiz)
        {
            CurrentQuiz = quiz;
            OnQuizUpdated?.Invoke(); // Notify subscribers that the quiz has been updated
            Console.WriteLine($"CurrentQuiz updated. Quiz ID: {quiz.Id}");
        }
    }
}
