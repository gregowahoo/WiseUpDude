using System;
using System.Collections.Generic;

namespace WiseUpDude.Model
{
    public class LearningTrackSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SourceType { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public int LearningTrackCategoryId { get; set; }
        public DateTime CreationDate { get; set; }
        public List<LearningTrackQuiz> Quizzes { get; set; } = new();
    }
}