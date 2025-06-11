namespace WiseUpDude.Model
{
    public class LearningTrackQuizAttempt
    {
        public int Id { get; set; }
        public int LearningTrackQuizId { get; set; }
        public LearningTrackQuiz? LearningTrackQuiz { get; set; }

        public DateTime AttemptDate { get; set; }
        public double Score { get; set; }
        public TimeSpan Duration { get; set; }

        public List<LearningTrackQuizAttemptQuestion> AttemptQuestions { get; set; } = new();

        public bool IsComplete { get; set; }
    }
}