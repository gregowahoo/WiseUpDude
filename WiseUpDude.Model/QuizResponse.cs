namespace WiseUpDude.Model
{
    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public string Type { get; set; } = string.Empty; // "Topic" or "Prompt"
        public string? Topic { get; set; } // Nullable for prompts
        public string? Prompt { get; set; } // Nullable for topics
        public string? Description { get; set; } // Optional
        public string Difficulty { get; set; } = string.Empty; // Optional
        public int TopicId { get; set; }
        public string? UserId { get; set; } 
    }
}
