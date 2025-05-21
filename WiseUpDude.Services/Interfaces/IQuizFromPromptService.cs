using WiseUpDude.Model;

namespace WiseUpDude.Services.Interfaces
{
    public interface IQuizFromPromptService
    {
        Task<List<QuizQuestion>?> GenerateQuestionsFromPromptAsync(string prompt);
    }
}
