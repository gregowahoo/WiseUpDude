namespace WiseUpDude.Model
{
    public class QuizModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<QuizQuestion> Questions { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
