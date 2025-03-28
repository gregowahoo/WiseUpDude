using System.ComponentModel.DataAnnotations;

namespace WiseUpDude.Model
{
    public enum QuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }

    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Question { get; set; }

        [Required]
        public QuizQuestionType QuestionType { get; set; }

        public List<string>? Options { get; set; }

        [Required]
        public string? Answer { get; set; }         // The correct answer

        public string? Explanation { get; set; }    // Explanation for the correct answer

        public string? UserAnswer { get; set; }       // To store the answer provided by the user

        [Required]
        public int QuizId { get; set; }

        public Quiz Quiz { get; set; }
    }


    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new();
    }
}
