using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizAttemptRepository : IUserQuizAttemptRepository<Model.UserQuizAttempt>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserQuizAttemptRepository> _logger;

        public UserQuizAttemptRepository(ApplicationDbContext context, ILogger<UserQuizAttemptRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Model.UserQuizAttempt?> GetByIdAsync(int id)
        {
            var entity = await _context.UserQuizAttempts
                .Include(a => a.AttemptQuestions)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
            {
                _logger.LogWarning($"UserQuizAttempt with Id {id} not found.");
                return null;
            }
            return new Model.UserQuizAttempt
            {
                Id = entity.Id,
                UserQuizId = entity.UserQuizId,
                AttemptDate = entity.AttemptDate,
                Score = entity.Score,
                Duration = entity.Duration,
                AttemptQuestions = entity.AttemptQuestions?.Select(q => new Model.UserQuizAttemptQuestion
                {
                    Id = q.Id,
                    UserQuizAttemptId = q.UserQuizAttemptId,
                    UserQuizQuestionId = q.UserQuizQuestionId,
                    UserAnswer = q.UserAnswer,
                    IsCorrect = q.IsCorrect,
                    TimeTakenSeconds = q.TimeTakenSeconds
                }).ToList()
            };
        }

        public async Task<IEnumerable<Model.UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId)
        {
            var attempts = await _context.UserQuizAttempts
                .Include(a => a.AttemptQuestions)
                .Where(a => a.UserQuizId == userQuizId)
                .ToListAsync();
            return attempts.Select(entity => new Model.UserQuizAttempt
            {
                Id = entity.Id,
                UserQuizId = entity.UserQuizId,
                AttemptDate = entity.AttemptDate,
                Score = entity.Score,
                Duration = entity.Duration,
                AttemptQuestions = entity.AttemptQuestions?.Select(q => new Model.UserQuizAttemptQuestion
                {
                    Id = q.Id,
                    UserQuizAttemptId = q.UserQuizAttemptId,
                    UserQuizQuestionId = q.UserQuizQuestionId,
                    UserAnswer = q.UserAnswer,
                    IsCorrect = q.IsCorrect,
                    TimeTakenSeconds = q.TimeTakenSeconds
                }).ToList()
            });
        }

        public async Task<Model.UserQuizAttempt> AddAsync(Model.UserQuizAttempt attempt)
        {
            var entity = new Entities.UserQuizAttempt
            {
                UserQuizId = attempt.UserQuizId,
                AttemptDate = attempt.AttemptDate,
                Score = attempt.Score,
                Duration = attempt.Duration
            };
            _context.UserQuizAttempts.Add(entity);
            await _context.SaveChangesAsync();

            if (attempt.AttemptQuestions != null && attempt.AttemptQuestions.Any())
            {
                entity.AttemptQuestions = attempt.AttemptQuestions.Select(q => new Entities.UserQuizAttemptQuestion
                {
                    UserQuizAttemptId = entity.Id,
                    UserQuizQuestionId = q.UserQuizQuestionId,
                    UserAnswer = q.UserAnswer,
                    IsCorrect = q.IsCorrect,
                    TimeTakenSeconds = q.TimeTakenSeconds
                }).ToList();
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            attempt.Id = entity.Id;
            _logger.LogInformation($"Added UserQuizAttempt with Id {entity.Id} and {entity.AttemptQuestions?.Count ?? 0} questions.");
            return attempt;
        }

        public async Task UpdateAsync(Model.UserQuizAttempt model)
        {
            var entity = await _context.UserQuizAttempts
                .Include(a => a.AttemptQuestions)
                .FirstOrDefaultAsync(a => a.Id == model.Id);
            if (entity == null)
            {
                _logger.LogWarning($"UserQuizAttempt with Id {model.Id} not found for update.");
                throw new KeyNotFoundException($"UserQuizAttempt with Id {model.Id} not found.");
            }
            entity.UserQuizId = model.UserQuizId;
            entity.AttemptDate = model.AttemptDate;
            entity.Score = model.Score;
            entity.Duration = model.Duration;
            // Update AttemptQuestions
            entity.AttemptQuestions = model.AttemptQuestions?.Select(q => new Entities.UserQuizAttemptQuestion
            {
                Id = q.Id,
                UserQuizAttemptId = entity.Id,
                UserQuizQuestionId = q.UserQuizQuestionId,
                UserAnswer = q.UserAnswer,
                IsCorrect = q.IsCorrect,
                TimeTakenSeconds = q.TimeTakenSeconds
            }).ToList();
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Updated UserQuizAttempt with Id {entity.Id}.");
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.UserQuizAttempts
                .Include(a => a.AttemptQuestions)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (entity != null)
            {
                _context.UserQuizAttempts.Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted UserQuizAttempt with Id {id}.");
            }
            else
            {
                _logger.LogWarning($"UserQuizAttempt with Id {id} not found for deletion.");
            }
        }
    }
}
