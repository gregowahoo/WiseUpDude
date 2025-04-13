using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Data.Entities;

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
                Questions = e.Questions.Select(q => new Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer
                }).ToList()
            }).ToList();
        }
    }
}

