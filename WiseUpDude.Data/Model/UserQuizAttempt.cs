namespace WiseUpDude.Model
{
    public class UserQuizAttempt
    {
        public int Id { get; set; }
        public int UserQuizId { get; set; }
        public DateTime AttemptDate { get; set; }
        public double Score { get; set; }  // e.g., percentage correct
        public TimeSpan Duration { get; set; }
        public List<UserQuizAttemptQuestion>? AttemptQuestions { get; set; }
    }
}
