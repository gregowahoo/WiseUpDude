using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Question { get; set; }

        [Required]
        public QuizQuestionType QuestionType { get; set; }

        public string? OptionsJson { get; set; } // Store options as JSON in the database

        [Required]
        public string? Answer { get; set; } // The correct answer

        public string? Explanation { get; set; } // Explanation for the correct answer

        public string? UserAnswer { get; set; } // To store the answer provided by the user

        [Required]
        public int QuizId { get; set; } // Foreign key to Quiz_Orig

        [ForeignKey("QuizId")]
        public Quiz Quiz { get; set; } // Navigation property to Quiz_Orig
    }

    public enum QuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }
}
