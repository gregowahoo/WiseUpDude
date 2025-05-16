namespace WiseUpDude.Model
{
    public class UserQuizAttemptQuestion
    {
        public int Id { get; set; }
        public int UserQuizAttemptId { get; set; }
        public int UserQuizQuestionId { get; set; }
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public double? TimeTakenSeconds { get; set; }
    }
}
