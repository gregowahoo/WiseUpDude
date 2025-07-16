using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class UserQuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Question { get; set; }

        [Required]
        public UserQuizQuestionType QuestionType { get; set; }

        public string? OptionsJson { get; set; }

        [Required]
        public string? Answer { get; set; }

        public string? Explanation { get; set; }

        public string? UserAnswer { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public required UserQuiz Quiz { get; set; }

        [Required]
        [MaxLength(50)]
        public string Difficulty { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; }

        public string? ContextSnippet { get; set; }
        public string? Citation { get; set; }
    }

    public enum UserQuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }

}
