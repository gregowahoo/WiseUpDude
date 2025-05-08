using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class UserQuiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public List<UserQuizQuestion> Questions { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [MaxLength(100)]
        public string? Topic { get; set; }

        public string? Prompt { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Difficulty { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; }

        // Change LearnMode to non-nullable
        public bool LearnMode { get; set; }
    }
}
