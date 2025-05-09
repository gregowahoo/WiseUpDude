namespace WiseUpDude.Data.Entities
{
    public class UserQuizAttempt
    {
        public int Id { get; set; }
        public int UserQuizId { get; set; }
        public UserQuiz UserQuiz { get; set; }

        public DateTime AttemptDate { get; set; }
        public double Score { get; set; }  // e.g., percentage correct
        public TimeSpan Duration { get; set; }

        public ICollection<UserQuizAttemptQuestion> AttemptQuestions { get; set; }
    }

}
