namespace WiseUpDude.Model
{
    public enum QuizQuestionType
    {
        TrueFalse,
        MultipleChoice
    }

    public class QuizQuestion
    {
        public string? Question { get; set; }
        public QuizQuestionType QuestionType { get; set; }
        public List<string>? Options { get; set; }
        public string? Answer { get; set; }         // The correct answer
        public string? Explanation { get; set; }    // Explanation for the correct answer
        public string? UserAnswer { get; set; }       // To store the answer provided by the user
    }

    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new();
    }
}
