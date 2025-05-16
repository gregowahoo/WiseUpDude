using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizAttemptRepository : IUserQuizAttemptRepository<Model.UserQuizAttempt>
    {
        private readonly ApplicationDbContext _context;

        public UserQuizAttemptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Model.UserQuizAttempt?> GetByIdAsync(int id)
        {
            var entity = await _context.UserQuizAttempts
                .Include(a => a.AttemptQuestions)
                .FirstOrDefaultAsync(a => a.Id == id);

            return entity == null ? null : MapToModel(entity);
        }

        public async Task<IEnumerable<Model.UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId)
        {
            var entities = await _context.UserQuizAttempts
                .Where(a => a.UserQuizId == userQuizId)
                .Include(a => a.AttemptQuestions)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        public async Task AddAsync(Model.UserQuizAttempt attempt)
        {
            try
            {
                var entity = MapToEntity(attempt);
                _context.UserQuizAttempts.Add(entity);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error adding UserQuizAttempt: {ex.Message}");
                throw; // Re-throw the exception to ensure it propagates
            }
        }

        public async Task UpdateAsync(Model.UserQuizAttempt attempt)
        {
            var entity = MapToEntity(attempt);
            _context.UserQuizAttempts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.UserQuizAttempts.FindAsync(id);
            if (entity != null)
            {
                _context.UserQuizAttempts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        private Model.UserQuizAttempt MapToModel(Entities.UserQuizAttempt model)
        {
            return new Model.UserQuizAttempt
            {
                Id = model.Id,
                UserQuizId = model.UserQuizId,
                AttemptDate = model.AttemptDate,
                Score = model.Score,
                Duration = model.Duration,
                AttemptQuestions = model.AttemptQuestions?.Select(MapToModel).ToList()
            };
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

        private Entities.UserQuizAttempt MapToEntity(Model.UserQuizAttempt model)
        {
            return new Entities.UserQuizAttempt
            {
                Id = model.Id,
                UserQuizId = model.UserQuizId,
                AttemptDate = model.AttemptDate,
                Score = model.Score,
                Duration = model.Duration,
                AttemptQuestions = model.AttemptQuestions?.Select(MapToEntity).ToList()
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