using System;
using System.Collections.Generic;

namespace WiseUpDude.Model
{
    public class LearningTrackCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public int LearningTrackId { get; set; }
        public DateTime CreationDate { get; set; }
        public List<LearningTrackSource> Sources { get; set; } = new();
    }
}