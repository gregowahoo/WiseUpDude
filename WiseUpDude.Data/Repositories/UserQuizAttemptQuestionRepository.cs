using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
//using WiseUpDude.Data.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizAttemptQuestionRepository : IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion>
    {
        private readonly ApplicationDbContext _context;

        public UserQuizAttemptQuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Model.UserQuizAttemptQuestion?> GetByIdAsync(int id)
        {
            var entity = await _context.UserQuizAttemptQuestions
                .FirstOrDefaultAsync(q => q.Id == id);

            return entity == null ? null : MapToModel(entity);
        }

        public async Task<IEnumerable<Model.UserQuizAttemptQuestion>> GetByAttemptIdAsync(int attemptId)
        {
            var entities = await _context.UserQuizAttemptQuestions
                .Where(q => q.UserQuizAttemptId == attemptId)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        public async Task AddAsync(Model.UserQuizAttemptQuestion question)
        {
            var entity = MapToEntity(question);
            _context.UserQuizAttemptQuestions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Model.UserQuizAttemptQuestion question)
        {
            var entity = MapToEntity(question);
            _context.UserQuizAttemptQuestions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.UserQuizAttemptQuestions.FindAsync(id);
            if (entity != null)
            {
                _context.UserQuizAttemptQuestions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        private Model.UserQuizAttemptQuestion MapToModel(Entities.UserQuizAttemptQuestion entity)
        {
            return new Model.UserQuizAttemptQuestion
            {
                Id = entity.Id,
                UserQuizAttemptId = entity.UserQuizAttemptId,
                UserQuizQuestionId = entity.UserQuizQuestionId,
                UserAnswer = entity.UserAnswer,
                IsCorrect = entity.IsCorrect,
                TimeTakenSeconds = entity.TimeTakenSeconds
            };
        }

        private Entities.UserQuizAttemptQuestion MapToEntity(Model.UserQuizAttemptQuestion model)
        {
            return new Entities.UserQuizAttemptQuestion
            {
                Id = model.Id,
                UserQuizAttemptId = model.UserQuizAttemptId,
                UserQuizQuestionId = model.UserQuizQuestionId,
                UserAnswer = model.UserAnswer,
                IsCorrect = model.IsCorrect,
                TimeTakenSeconds = model.TimeTakenSeconds
            };
        }
    }
}