using WiseUpDude.Services;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class QuizStateService
    {
        public QuizResponse? CurrentQuiz { get; set; }
        public required QuizSource QuizSource { get; set; } = new QuizSource();
    }
}
