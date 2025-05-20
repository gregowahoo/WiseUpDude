using WiseUpDude.Model;

namespace WiseUpDude.Services.Interfaces
{
    public interface IQuizFromPromptService
    {
        Task<int?> GenerateQuizFromPromptAndSaveAsync(string prompt, string userName = "greg.ohlsen@gmail.com");
    }
}
