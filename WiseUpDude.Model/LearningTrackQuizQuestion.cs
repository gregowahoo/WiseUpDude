using System;

namespace WiseUpDude.Model
{
    public class LearningTrackQuizQuestion
    {
        public int Id { get; set; }
        public int LearningTrackQuizId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public string? OptionsJson { get; set; }
        public string? Difficulty { get; set; }
        public DateTime CreationDate { get; set; }
    }
}