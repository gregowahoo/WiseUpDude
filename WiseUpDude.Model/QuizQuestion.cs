using System.Text.Json;

namespace WiseUpDude.Model
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public QuizQuestionType QuestionType { get; set; }
        public List<string>? Options { get; set; } // POCO-friendly property
        public string? Answer { get; set; }
        public string? Explanation { get; set; }
        public string? UserAnswer { get; set; }
        public int QuizId { get; set; }

        // Convert OptionsJson to List<string> and vice versa
        public string OptionsJson
        {
            get => Options == null ? string.Empty : JsonSerializer.Serialize(Options);
            set => Options = string.IsNullOrEmpty(value) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(value);
        }
        public string Difficulty { get; set; } = string.Empty;

        public string? ContextSnippet { get; set; }
        //public string? Citation { get; set; }
        public List<string>? Citation { get; set; }
    }

    public enum QuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }
}
