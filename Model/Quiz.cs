namespace WiseUpDude.Model
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        public string UserName { get; set; }
    }
}