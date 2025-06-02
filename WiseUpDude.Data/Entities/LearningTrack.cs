using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class LearningTrack
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<LearningTrackCategory> Categories { get; set; } = new List<LearningTrackCategory>();
    }

    public class LearningTrackCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? Difficulty { get; set; }
        [Required]
        public int LearningTrackId { get; set; }
        [ForeignKey("LearningTrackId")]
        public LearningTrack LearningTrack { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<LearningTrackSource> Sources { get; set; } = new List<LearningTrackSource>();
    }

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
