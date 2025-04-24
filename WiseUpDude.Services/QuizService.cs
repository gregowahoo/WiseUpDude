using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data;

namespace WiseUpDude.Services
{
    public class QuizService
    {
        private readonly IUserRepository<Data.Entities.Quiz> _quizRepository;

        public QuizService(IUserRepository<Data.Entities.Quiz> quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<List<Model.Quiz>> GetAllQuizzesAsync()
        {
            var entities = await _quizRepository.GetAllAsync();
            return entities.Select(e => new Model.Quiz
            {
                Id = e.Id,
                Name = e.Name,
                UserName = e.User?.UserName ?? "Unknown User", // Handle possible null reference
                User = e.User ?? new ApplicationUser { UserName = "Unknown User" }, // Provide a default User object
                Questions = e.Questions.Select(q => new Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    QuestionType = (Model.QuizQuestionType)q.QuestionType,
                    Options = string.IsNullOrEmpty(q.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.OptionsJson),
                    Difficulty = q.Difficulty // Include question-level difficulty
                }).ToList(),
                Type = e.Type,
                Topic = e.Topic,
                Prompt = e.Prompt,
                Description = e.Description,
                Difficulty = e.Difficulty // Include quiz-level difficulty
            }).ToList();
        }
    }
}

