using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class LearningTrackQuiz
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Required]
        public int LearningTrackSourceId { get; set; }
        [ForeignKey("LearningTrackSourceId")]
        public LearningTrackSource LearningTrackSource { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<LearningTrackQuizQuestion> Questions { get; set; } = new List<LearningTrackQuizQuestion>();
    }
}
