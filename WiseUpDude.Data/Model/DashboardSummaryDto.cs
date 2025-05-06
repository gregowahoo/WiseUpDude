namespace WiseUpDude.Model
{
    public class DashboardSummaryDto
    {
        public int TotalQuizzesTaken { get; set; }
        public double AverageScore { get; set; }
        public string? BestQuizName { get; set; }
        public double? BestQuizScore { get; set; }
        public List<RecentQuizDto> RecentQuizzes { get; set; } = new();
    }
}
