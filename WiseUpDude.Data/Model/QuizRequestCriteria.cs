namespace WiseUpDude.Model
{
    public class QuizRequestCriteria
    {
        public string Topic { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";

        // Additional filters can go here in the future
        // e.g. public string QuestionType { get; set; }
    }
}
