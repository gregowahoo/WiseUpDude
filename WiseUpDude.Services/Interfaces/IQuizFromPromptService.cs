using WiseUpDude.Model;

namespace WiseUpDude.Services.Interfaces
{
    public interface IQuizFromPromptService
    {
        Task<QuizResponse?> GenerateQuizFromPromptAsync(string prompt);
    }
}
