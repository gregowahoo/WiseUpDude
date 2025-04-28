using WiseUpDude.Services;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class QuizStateService
    {
        public Quiz? CurrentQuiz { get; set; }

        //// Removed QuizSource and updated to use QuizResponse properties directly
        //public string Type { get; set; } = string.Empty; // "Topic" or "Prompt"
        //public string? Topic { get; set; } // Nullable for prompts
        //public string? Prompt { get; set; } // Nullable for topics
        //public string? Description { get; set; } // Optional
    }
}
