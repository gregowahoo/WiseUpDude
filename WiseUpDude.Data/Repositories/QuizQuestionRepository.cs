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
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public QuizQuestionRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<WiseUpDude.Model.QuizQuestion>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entities = await context.QuizQuestions.ToListAsync();
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
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
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            // Fetch the associated Quiz entity to set the required Quiz property
            var quizEntity = await context.Quizzes.FirstOrDefaultAsync(q => q.Id == model.QuizId);
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

            context.QuizQuestions.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WiseUpDude.Model.QuizQuestion model)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == model.Id);
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

            context.Entry(entity).State = EntityState.Modified; // Mark entity as modified
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await context.QuizQuestions.FindAsync(id);
            if (entity != null)
            {
                context.QuizQuestions.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        //public Task<IEnumerable<Model.Quiz>> GetQuizzesByTopicIdAsync(int topicId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
