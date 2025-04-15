namespace WiseUpDude.Model
{
    public class QuizResponse
    {
        public List<QuizQuestion> Questions { get; set; } = new();
        public required QuizSource QuizSource { get; set; }
    }
}
