namespace WiseUpDude.Model
{
    public class RecentQuizDto
    {
        public int Id { get; set; } // New property
        public string Name { get; set; }
        public double Score { get; set; }
        public string? Type { get; set; }
        public string? Topic { get; set; }
        public string? Prompt { get; set; }
        public string? Description { get; set; }

        // New property for LearnMode
        public bool LearnMode { get; set; }
    }
}