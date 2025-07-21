using System.Text.Json;
using WiseUpDude.Model; // Use model's CitationMeta

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

        public List<CitationMeta>? Citation { get; set; }
        public string CitationJson
        {
            get => Citation == null ? string.Empty : JsonSerializer.Serialize(Citation);
            set => Citation = string.IsNullOrEmpty(value) ? new List<CitationMeta>() : JsonSerializer.Deserialize<List<CitationMeta>>(value);
        }
    }

    public enum QuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }
}
