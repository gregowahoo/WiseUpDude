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
        private readonly IRepository<Data.Entities.Quiz> _quizRepository;

        public QuizService(IRepository<Data.Entities.Quiz> quizRepository)
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
                    Answer = q.Answer
                }).ToList(),
                QuizSource = new Model.QuizSource // Map the QuizSource entity
                {
                    Id = e.QuizSource?.Id ?? 0, // Handle possible null reference
                    Type = e.QuizSource?.Type ?? "Unknown Type",
                    Topic = e.QuizSource?.Topic ?? "Unknown Topic",
                    Prompt = e.QuizSource?.Prompt,
                    Description = e.QuizSource?.Description ?? "No description available."
                }
            }).ToList();
        }
    }
}

