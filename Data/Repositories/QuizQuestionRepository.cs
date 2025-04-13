using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class QuizQuestionRepository : IRepository<Model.QuizQuestion>
    {
        private readonly ApplicationDbContext _context;

        public QuizQuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Model.QuizQuestion>> GetAllAsync()
        {
            var entities = await _context.QuizQuestions.ToListAsync();
            return entities.Select(e => new Model.QuizQuestion
            {
                Id = e.Id,
                Question = e.Question,
                QuestionType = e.QuestionType,
                Options = string.IsNullOrEmpty(e.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(e.OptionsJson),
                Answer = e.Answer,
                Explanation = e.Explanation,
                UserAnswer = e.UserAnswer,
                QuizId = e.QuizId
            });
        }

        public async Task<Model.QuizQuestion> GetByIdAsync(int id)
        {
            var entity = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"QuizQuestion with Id {id} not found.");

            return new Model.QuizQuestion
            {
                Id = entity.Id,
                Question = entity.Question,
                QuestionType = entity.QuestionType,
                Options = string.IsNullOrEmpty(entity.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.OptionsJson),
                Answer = entity.Answer,
                Explanation = entity.Explanation,
                UserAnswer = entity.UserAnswer,
                QuizId = entity.QuizId
            };
        }

        public async Task AddAsync(Model.QuizQuestion model)
        {
            var entity = new Data.Entities.QuizQuestion
            {
                Question = model.Question,
                QuestionType = model.QuestionType,
                OptionsJson = model.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(model.Options),
                Answer = model.Answer,
                Explanation = model.Explanation,
                UserAnswer = model.UserAnswer,
                QuizId = model.QuizId
            };

            _context.QuizQuestions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Model.QuizQuestion model)
        {
            var entity = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == model.Id);
            if (entity == null)
                throw new KeyNotFoundException($"QuizQuestion with Id {model.Id} not found.");

            entity.Question = model.Question;
            entity.QuestionType = model.QuestionType;
            entity.OptionsJson = model.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(model.Options);
            entity.Answer = model.Answer;
            entity.Explanation = model.Explanation;
            entity.UserAnswer = model.UserAnswer;
            entity.QuizId = model.QuizId;

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.QuizQuestions.FindAsync(id);
            if (entity != null)
            {
                _context.QuizQuestions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
