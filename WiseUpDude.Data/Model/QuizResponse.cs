namespace WiseUpDude.Model
{
    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new();

        // Moved properties from QuizSource
        public string Type { get; set; } = string.Empty; // "Topic" or "Prompt"
        public string? Topic { get; set; } // Nullable for prompts
        public string? Prompt { get; set; } // Nullable for topics
        public string? Description { get; set; } // Optional
    }
}
