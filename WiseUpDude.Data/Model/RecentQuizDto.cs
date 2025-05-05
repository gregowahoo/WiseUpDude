namespace WiseUpDude.Model
{
    public class RecentQuizDto
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public string? Type { get; set; } // Ensure Type is included
        public string? Topic { get; set; }
        public string? Prompt { get; set; }
        public string? Description { get; set; }
    }
}