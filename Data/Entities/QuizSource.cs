using System.ComponentModel.DataAnnotations;

namespace WiseUpDude.Data.Entities
{
    public class QuizSource
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "Topic" or "Prompt"

        [MaxLength(100)]
        public string? Topic { get; set; } // Nullable for prompts

        public string? Prompt { get; set; } // Nullable for topics

        [MaxLength(500)]
        public string? Description { get; set; } // Optional
    }
}
