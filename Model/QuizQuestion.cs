﻿namespace WiseUpDude.Model
{
    public enum QuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }

    public class QuizQuestion
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public QuizQuestionType QuestionType { get; set; }
        public List<string>? Options { get; set; }
        public string? Answer { get; set; }         // The correct answer
        public string? Explanation { get; set; }    // Explanation for the correct answer
        public string? UserAnswer { get; set; }       // To store the answer provided by the user

        public int QuizId { get; set; } // Foreign key to reference QuizModel entity
        public QuizModel Quiz { get; set; } // Navigation property to reference QuizModel entity
    }

    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new();
    }
}
