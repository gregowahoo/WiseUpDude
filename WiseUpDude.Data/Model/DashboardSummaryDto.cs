namespace WiseUpDude.Data.Model
{
    public class DashboardSummaryDto
    {
        public int TotalQuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        public string? BestQuizName { get; set; }
        public double? BestQuizScore { get; set; }
        public List<RecentQuizDto> RecentQuizzes { get; set; } = new();
    }

    public class RecentQuizDto
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public string? Type { get; set; }
        public string? Topic { get; set; }
        public string? Prompt { get; set; }
        public string? Description { get; set; }
    }
}
