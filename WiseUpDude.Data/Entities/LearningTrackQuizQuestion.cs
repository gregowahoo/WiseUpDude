using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class LearningTrackQuizQuestion
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LearningTrackQuizId { get; set; }
        [ForeignKey("LearningTrackQuizId")]
        public LearningTrackQuiz LearningTrackQuiz { get; set; }
        [Required]
        public string Question { get; set; } = string.Empty;
        [Required]
        public string Answer { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Explanation { get; set; }
        [MaxLength(500)]
        public string? OptionsJson { get; set; }
        [MaxLength(50)]
        public string? Difficulty { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
