using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WiseUpDude.Data;

namespace WiseUpDude.Model
{
    public class Quiz_Orig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public List<QuizQuestion_Orig> Questions { get; set; } = new();

        // Foreign key for ApplicationUser
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
