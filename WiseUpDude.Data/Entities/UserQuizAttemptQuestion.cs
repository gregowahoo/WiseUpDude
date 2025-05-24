namespace WiseUpDude.Data.Entities
{
    public class UserQuizAttemptQuestion
    {
        public int Id { get; set; }

        public int UserQuizAttemptId { get; set; }
        public UserQuizAttempt UserQuizAttempt { get; set; }

        public int UserQuizQuestionId { get; set; }
        // Removing the navigation property to UserQuizQuestion
        // public UserQuizQuestion UserQuizQuestion { get; set; }

        // User's answer data
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public double? TimeTakenSeconds { get; set; }
    }

}
