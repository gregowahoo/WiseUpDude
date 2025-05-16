using System.Collections.Generic;
// Ensure this repository is used only for function-generated quiz questions.
// No changes needed here unless specific logic is required to differentiate usage.
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class QuizQuestionRepository : IQuizQuestionRepository<WiseUpDude.Model.QuizQuestion>
    {
        private readonly ApplicationDbContext _context;

        public QuizQuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WiseUpDude.Model.QuizQuestion>> GetAllAsync()
        {
            var entities = await _context.QuizQuestions.ToListAsync();
            return entities.Select(e => new WiseUpDude.Model.QuizQuestion
            {
                Id = e.Id,
                Question = e.Question,
                QuestionType = (WiseUpDude.Model.QuizQuestionType)e.QuestionType, // Explicit cast
                Options = string.IsNullOrEmpty(e.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(e.OptionsJson),
                Answer = e.Answer,
                Explanation = e.Explanation,
                UserAnswer = e.UserAnswer,
                QuizId = e.QuizId
            });
        }

        public async Task<WiseUpDude.Model.QuizQuestion> GetByIdAsync(int id)
        {
            var entity = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"QuizQuestion with Id {id} not found.");

            return new WiseUpDude.Model.QuizQuestion
            {
                Id = entity.Id,
                Question = entity.Question,
                QuestionType = (WiseUpDude.Model.QuizQuestionType)entity.QuestionType, // Explicit cast
                Options = string.IsNullOrEmpty(entity.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(entity.OptionsJson),
                Answer = entity.Answer,
                Explanation = entity.Explanation,
                UserAnswer = entity.UserAnswer,
                QuizId = entity.QuizId
            };
        }

        public async Task AddAsync(WiseUpDude.Model.QuizQuestion model)
        {
            // Fetch the associated Quiz entity to set the required Quiz property
            var quizEntity = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == model.QuizId);
            if (quizEntity == null)
            {
                throw new KeyNotFoundException($"Quiz with Id {model.QuizId} not found.");
            }

            var entity = new Entities.QuizQuestion
            {
                Question = model.Question,
                QuestionType = (Entities.QuizQuestionType)model.QuestionType, // Explicit cast
                OptionsJson = model.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(model.Options),
                Answer = model.Answer,
                Explanation = model.Explanation,
                UserAnswer = model.UserAnswer,
                QuizId = model.QuizId,
                Quiz = quizEntity // Set the required Quiz property
            };

            _context.QuizQuestions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WiseUpDude.Model.QuizQuestion model)
        {
            var entity = await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == model.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"QuizQuestion with Id {model.Id} not found.");
            }

            // Map the model to the entity
            entity.Question = model.Question;
            entity.QuestionType = (Entities.QuizQuestionType)model.QuestionType; // Explicit cast
            entity.OptionsJson = model.Options != null ? System.Text.Json.JsonSerializer.Serialize(model.Options) : null; // Serialize options
            entity.Answer = model.Answer;
            entity.Explanation = model.Explanation;
            entity.UserAnswer = model.UserAnswer; // Ensure UserAnswer is updated
            entity.QuizId = model.QuizId;

            _context.Entry(entity).State = EntityState.Modified; // Mark entity as modified
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

        //public Task<IEnumerable<Model.Quiz>> GetQuizzesByTopicIdAsync(int topicId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
