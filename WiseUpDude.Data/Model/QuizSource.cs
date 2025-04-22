namespace WiseUpDude.Model
{
    public class QuizSource
    {
        public int Id { get; set; }

        public string Type { get; set; } = string.Empty; // "Topic" or "Prompt"

        public string? Topic { get; set; } // Nullable for prompts

        public string? Prompt { get; set; } // Nullable for topics

        public string? Description { get; set; } // Optional
    }
}
