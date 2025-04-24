namespace WiseUpDude.Model
{
    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new();

        public string Type { get; set; } = string.Empty; // "Topic" or "Prompt"
        public string? Topic { get; set; } // Nullable for prompts
        public string? Prompt { get; set; } // Nullable for topics
        public string? Description { get; set; } // Optional

        // Add Difficulty for quiz-level
        public string Difficulty { get; set; } = string.Empty;
    }
}
