namespace WiseUpDude.Data.Entities
{
    public class LearningTrackAttemptQuestion
    {
        public int Id { get; set; }

        public int LearningTrackAttemptId { get; set; }
        public LearningTrackQuizAttempt LearningTrackAttempt { get; set; }

        public int LearningTrackQuestionId { get; set; }
        // Navigation property to LearningTrackQuestion can be added if needed
        // public LearningTrackQuestion LearningTrackQuestion { get; set; }

        // User's answer data
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public double? TimeTakenSeconds { get; set; }
    }
}
