namespace WiseUpDude.Data.Entities
{
    public class LearningTrackQuizAttempt
    {
        public int Id { get; set; }
        public int LearningTrackQuizId { get; set; }
        public LearningTrackQuiz LearningTrackQuiz { get; set; }

        public DateTime AttemptDate { get; set; }
        public double Score { get; set; }  // e.g., percentage correct
        public TimeSpan Duration { get; set; }

        public ICollection<LearningTrackQuizAttemptQuestion> AttemptQuestions { get; set; }

        // Indicates if the attempt is complete (all questions answered)
        public bool IsComplete { get; set; }
    }
}
