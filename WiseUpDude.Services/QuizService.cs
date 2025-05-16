using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Services
{
    public class QuizService
    {
        private readonly IUserQuizRepository<Data.Entities.Quiz> _quizRepository;

        public QuizService(IUserQuizRepository<Data.Entities.Quiz> quizRepository)
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
                UserId = e.User?.Id ?? "Unknown User Id", // Handle possible null reference
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
                Topic = e.Topic?.Name, // Extract the Topic's Name
                Prompt = e.Prompt,
                Description = e.Description,
                Difficulty = e.Difficulty, // Include quiz-level difficulty
                //LearnMode = e.LearnMode // Include LearnMode
            }).ToList();
        }

        public async Task UpdateLearnModeAsync(int quizId, bool learnMode)
        {
            await _quizRepository.UpdateLearnModeAsync(quizId, learnMode);
        }
    }
}

