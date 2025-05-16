using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public List<QuizQuestion> Questions { get; set; } = new();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // "Topic" or "Prompt"

        //[MaxLength(100)]
        //public string? Topic { get; set; }

        public string? Prompt { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Add Difficulty property here
        [Required]
        [MaxLength(50)]
        public string Difficulty { get; set; } = string.Empty;

        [ForeignKey("Topic")]
        public int TopicId { get; set; } // Foreign key to Topic

        public Topic Topic { get; set; } = null!; // Navigation property

        public DateTime CreationDate { get; set; }
    }
}


