using WiseUpDude.Model;

namespace WiseUpDude.Services.Interfaces
{
    public interface IQuizGenerationService
    {
        Task<QuizResponse?> GenerateQuizFromPromptAsync(string prompt);
    }
}
