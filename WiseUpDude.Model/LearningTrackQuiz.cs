using System;
using System.Collections.Generic;

namespace WiseUpDude.Model
{
    public class LearningTrackQuiz
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LearningTrackSourceId { get; set; }
        public DateTime CreationDate { get; set; }
        public List<LearningTrackQuizQuestion> Questions { get; set; } = new();
    }
}