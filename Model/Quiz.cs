using WiseUpDude.Data;
using WiseUpDude.Data.Entities;

namespace WiseUpDude.Model
{
    public class Quiz
    {
        public int Id { get; set; }

        // Use the required modifier to ensure this property is initialized
        public required string Name { get; set; }

        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        // Use the required modifier to ensure this property is initialized
        public required string UserName { get; set; }

        // Use the required modifier to ensure this property is initialized
        public required ApplicationUser User { get; set; }

        // Use the required modifier to ensure this property is initialized
        public required QuizSource QuizSource { get; set; }
    }

}