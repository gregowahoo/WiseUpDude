namespace WiseUpDude.Model
{
    public class LearningTrackQuizAttemptQuestion
    {
        public int Id { get; set; }

        public int LearningTrackAttemptId { get; set; }
        public LearningTrackQuizAttempt? LearningTrackAttempt { get; set; }

        public int LearningTrackQuestionId { get; set; }
        // Optionally: public LearningTrackQuestion? LearningTrackQuestion { get; set; }

        public string? UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public double? TimeTakenSeconds { get; set; }
    }
}