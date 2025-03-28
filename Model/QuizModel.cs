namespace WiseUpDude.Model
{
    public class QuizModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<QuizQuestion> Questions { get; set; } = new();
    }
}
