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
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<UserQuizAttemptRepository> _logger;

        public UserQuizAttemptRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<UserQuizAttemptRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Model.UserQuizAttempt?> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizAttemptEntity = await context.UserQuizAttempts
                .Include(userQuizAttempt => userQuizAttempt.AttemptQuestions)
                .FirstOrDefaultAsync(userQuizAttempt => userQuizAttempt.Id == id);
            if (userQuizAttemptEntity == null)
            {
                _logger.LogWarning($"UserQuizAttempt with Id {id} not found.");
                return null;
            }
            return new Model.UserQuizAttempt
            {
                Id = userQuizAttemptEntity.Id,
                UserQuizId = userQuizAttemptEntity.UserQuizId,
                AttemptDate = userQuizAttemptEntity.AttemptDate,
                Score = userQuizAttemptEntity.Score,
                Duration = userQuizAttemptEntity.Duration,
                IsComplete = userQuizAttemptEntity.IsComplete,
                AttemptQuestions = userQuizAttemptEntity.AttemptQuestions?
                    .Select(attemptQuestionEntity => new Model.UserQuizAttemptQuestion
                    {
                        Id = attemptQuestionEntity.Id,
                        UserQuizAttemptId = attemptQuestionEntity.UserQuizAttemptId,
                        UserQuizQuestionId = attemptQuestionEntity.UserQuizQuestionId,
                        UserAnswer = attemptQuestionEntity.UserAnswer,
                        IsCorrect = attemptQuestionEntity.IsCorrect,
                        TimeTakenSeconds = attemptQuestionEntity.TimeTakenSeconds
                    }).ToList() ?? new List<Model.UserQuizAttemptQuestion>()
            };
        }

        public async Task<IEnumerable<Model.UserQuizAttempt>> GetByUserQuizIdAsync(int userQuizId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizAttemptEntities = await context.UserQuizAttempts
                .Include(userQuizAttempt => userQuizAttempt.AttemptQuestions)
                .Where(userQuizAttempt => userQuizAttempt.UserQuizId == userQuizId)
                .ToListAsync();
            return userQuizAttemptEntities.Select(userQuizAttemptEntity => new Model.UserQuizAttempt
            {
                Id = userQuizAttemptEntity.Id,
                UserQuizId = userQuizAttemptEntity.UserQuizId,
                AttemptDate = userQuizAttemptEntity.AttemptDate,
                Score = userQuizAttemptEntity.Score,
                Duration = userQuizAttemptEntity.Duration,
                IsComplete = userQuizAttemptEntity.IsComplete,
                AttemptQuestions = userQuizAttemptEntity.AttemptQuestions?
                    .Select(attemptQuestionEntity => new Model.UserQuizAttemptQuestion
                    {
                        Id = attemptQuestionEntity.Id,
                        UserQuizAttemptId = attemptQuestionEntity.UserQuizAttemptId,
                        UserQuizQuestionId = attemptQuestionEntity.UserQuizQuestionId,
                        UserAnswer = attemptQuestionEntity.UserAnswer,
                        IsCorrect = attemptQuestionEntity.IsCorrect,
                        TimeTakenSeconds = attemptQuestionEntity.TimeTakenSeconds
                    }).ToList() ?? new List<Model.UserQuizAttemptQuestion>()
            });
        }

        public async Task<Model.UserQuizAttempt> AddAsync(Model.UserQuizAttempt userQuizAttemptModel)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizAttemptEntity = new Entities.UserQuizAttempt
            {
                UserQuizId = userQuizAttemptModel.UserQuizId,
                AttemptDate = userQuizAttemptModel.AttemptDate,
                Score = userQuizAttemptModel.Score,
                Duration = userQuizAttemptModel.Duration,
                IsComplete = userQuizAttemptModel.IsComplete
            };
            context.UserQuizAttempts.Add(userQuizAttemptEntity);
            await context.SaveChangesAsync();

            if (userQuizAttemptModel.AttemptQuestions != null && userQuizAttemptModel.AttemptQuestions.Any())
            {
                userQuizAttemptEntity.AttemptQuestions = userQuizAttemptModel.AttemptQuestions.Select(attemptQuestionModel => new Entities.UserQuizAttemptQuestion
                {
                    UserQuizAttemptId = userQuizAttemptEntity.Id,
                    UserQuizQuestionId = attemptQuestionModel.UserQuizQuestionId,
                    UserAnswer = attemptQuestionModel.UserAnswer,
                    IsCorrect = attemptQuestionModel.IsCorrect,
                    TimeTakenSeconds = attemptQuestionModel.TimeTakenSeconds
                }).ToList();
                context.Entry(userQuizAttemptEntity).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            userQuizAttemptModel.Id = userQuizAttemptEntity.Id;
            _logger.LogInformation($"Added UserQuizAttempt with Id {userQuizAttemptEntity.Id} and {(userQuizAttemptEntity.AttemptQuestions?.Count ?? 0)} questions.");
            return userQuizAttemptModel;
        }

        public async Task UpdateAsync(Model.UserQuizAttempt userQuizAttemptModel)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizAttemptEntity = await context.UserQuizAttempts
                .Include(userQuizAttempt => userQuizAttempt.AttemptQuestions)
                .FirstOrDefaultAsync(userQuizAttempt => userQuizAttempt.Id == userQuizAttemptModel.Id);
            if (userQuizAttemptEntity == null)
            {
                _logger.LogWarning($"UserQuizAttempt with Id {userQuizAttemptModel.Id} not found for update.");
                throw new KeyNotFoundException($"UserQuizAttempt with Id {userQuizAttemptModel.Id} not found.");
            }
            userQuizAttemptEntity.UserQuizId = userQuizAttemptModel.UserQuizId;
            userQuizAttemptEntity.AttemptDate = userQuizAttemptModel.AttemptDate;
            userQuizAttemptEntity.Score = userQuizAttemptModel.Score;
            userQuizAttemptEntity.Duration = userQuizAttemptModel.Duration;
            userQuizAttemptEntity.IsComplete = userQuizAttemptModel.IsComplete;
            // Update AttemptQuestions
            userQuizAttemptEntity.AttemptQuestions = userQuizAttemptModel.AttemptQuestions?.Select(attemptQuestionModel => new Entities.UserQuizAttemptQuestion
            {
                Id = attemptQuestionModel.Id,
                UserQuizAttemptId = userQuizAttemptEntity.Id,
                UserQuizQuestionId = attemptQuestionModel.UserQuizQuestionId,
                UserAnswer = attemptQuestionModel.UserAnswer,
                IsCorrect = attemptQuestionModel.IsCorrect,
                TimeTakenSeconds = attemptQuestionModel.TimeTakenSeconds
            }).ToList() ?? new List<Entities.UserQuizAttemptQuestion>();
            context.Entry(userQuizAttemptEntity).State = EntityState.Modified;
            await context.SaveChangesAsync();
            _logger.LogInformation($"Updated UserQuizAttempt with Id {userQuizAttemptEntity.Id}.");
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizAttemptEntity = await context.UserQuizAttempts
                .Include(userQuizAttempt => userQuizAttempt.AttemptQuestions)
                .FirstOrDefaultAsync(userQuizAttempt => userQuizAttempt.Id == id);
            if (userQuizAttemptEntity != null)
            {
                context.UserQuizAttempts.Remove(userQuizAttemptEntity);
                await context.SaveChangesAsync();
                _logger.LogInformation($"Deleted UserQuizAttempt with Id {id}.");
            }
            else
            {
                _logger.LogWarning($"UserQuizAttempt with Id {id} not found for deletion.");
            }
        }
    }
}
