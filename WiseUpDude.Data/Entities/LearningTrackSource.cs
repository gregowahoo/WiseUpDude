using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class LearningTrackSource
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? SourceType { get; set; }
        [MaxLength(500)]
        public string? Url { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Required]
        public int LearningTrackCategoryId { get; set; }
        [ForeignKey("LearningTrackCategoryId")]
        public LearningTrackCategory LearningTrackCategory { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<LearningTrackQuiz> Quizzes { get; set; } = new List<LearningTrackQuiz>();
    }
}
