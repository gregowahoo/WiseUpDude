using System.ComponentModel.DataAnnotations;

namespace WiseUpDude.Model
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public List<QuizQuestion> Questions { get; set; } = new();
    }
}