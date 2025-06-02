using System;
using System.Collections.Generic;

namespace WiseUpDude.Model
{
    public class LearningTrack
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public List<LearningTrackCategory> Categories { get; set; } = new();
    }
}