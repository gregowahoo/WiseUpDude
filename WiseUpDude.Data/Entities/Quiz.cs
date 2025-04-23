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

        public List<QuizQuestion> Questions { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Navigation property for the user

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // "Topic" or "Prompt"

        [MaxLength(100)]
        public string? Topic { get; set; } // Nullable for prompts

        public string? Prompt { get; set; } // Nullable for topics

        [MaxLength(500)]
        public string? Description { get; set; } // Optional
    }
}


