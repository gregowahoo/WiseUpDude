namespace WiseUpDude.Model
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string UserId { get; set; }
        public List<QuizQuestion> Questions { get; set; }
    }
}
