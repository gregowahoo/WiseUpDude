using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizAttemptQuestionRepository : IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserQuizAttemptQuestionRepository> _logger;

        public UserQuizAttemptQuestionRepository(ApplicationDbContext context, ILogger<UserQuizAttemptQuestionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Model.UserQuizAttemptQuestion?> GetByIdAsync(int id)
        {
            _logger.LogInformation("GetByIdAsync: Fetching UserQuizAttemptQuestion with ID={Id}", id);
            
            try
            {
                var entity = await _context.UserQuizAttemptQuestions
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (entity == null)
                {
                    _logger.LogWarning("GetByIdAsync: UserQuizAttemptQuestion with ID={Id} not found", id);
                    return null;
                }

                var model = MapToModel(entity);
                _logger.LogInformation("GetByIdAsync: Successfully retrieved UserQuizAttemptQuestion with ID={Id}", id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync: Error retrieving UserQuizAttemptQuestion with ID={Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Model.UserQuizAttemptQuestion>> GetByAttemptIdAsync(int attemptId)
        {
            _logger.LogInformation("GetByAttemptIdAsync: Fetching UserQuizAttemptQuestions for attemptId={AttemptId}", attemptId);
            
            try
            {
                var entities = await _context.UserQuizAttemptQuestions
                    .Where(q => q.UserQuizAttemptId == attemptId)
                    .ToListAsync();

                _logger.LogInformation("GetByAttemptIdAsync: Retrieved {Count} UserQuizAttemptQuestions for attemptId={AttemptId}", 
                    entities.Count, attemptId);
                return entities.Select(MapToModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByAttemptIdAsync: Error retrieving UserQuizAttemptQuestions for attemptId={AttemptId}: {Message}", 
                    attemptId, ex.Message);
                throw;
            }
        }

        public async Task AddAsync(Model.UserQuizAttemptQuestion question)
        {
            _logger.LogInformation("AddAsync: Adding UserQuizAttemptQuestion for attemptId={AttemptId}, questionId={QuestionId}", 
                question.UserQuizAttemptId, question.UserQuizQuestionId);
            
            try
            {
                _logger.LogDebug("AddAsync: Question details: {Details}", JsonSerializer.Serialize(question));
                
                var entity = MapToEntity(question);
                _context.UserQuizAttemptQuestions.Add(entity);
                await _context.SaveChangesAsync();
                
                question.Id = entity.Id; // Update the ID in the original model
                _logger.LogInformation("AddAsync: Successfully added UserQuizAttemptQuestion with ID={Id}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddAsync: Error adding UserQuizAttemptQuestion: {Message}", ex.Message);
                throw;
            }
        }

        public async Task UpdateAsync(Model.UserQuizAttemptQuestion question)
        {
            _logger.LogInformation("UpdateAsync: Updating UserQuizAttemptQuestion with ID={Id}", question.Id);
            
            try
            {
                _logger.LogDebug("UpdateAsync: Question details: {Details}", JsonSerializer.Serialize(question));
                
                var entity = MapToEntity(question);
                _context.UserQuizAttemptQuestions.Update(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("UpdateAsync: Successfully updated UserQuizAttemptQuestion with ID={Id}", question.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync: Error updating UserQuizAttemptQuestion with ID={Id}: {Message}", 
                    question.Id, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("DeleteAsync: Deleting UserQuizAttemptQuestion with ID={Id}", id);
            
            try
            {
                var entity = await _context.UserQuizAttemptQuestions.FindAsync(id);
                if (entity != null)
                {
                    _context.UserQuizAttemptQuestions.Remove(entity);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("DeleteAsync: Successfully deleted UserQuizAttemptQuestion with ID={Id}", id);
                }
                else
                {
                    _logger.LogWarning("DeleteAsync: UserQuizAttemptQuestion with ID={Id} not found, nothing to delete", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync: Error deleting UserQuizAttemptQuestion with ID={Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<UserQuizAttemptQuestion> CreateOrUpdateAsync(UserQuizAttemptQuestion question)
        {
            _logger.LogInformation("CreateOrUpdateAsync: Creating/updating UserQuizAttemptQuestion for attemptId={AttemptId}, questionId={QuestionId}", 
                question.UserQuizAttemptId, question.UserQuizQuestionId);
            
            try
            {
                _logger.LogDebug("CreateOrUpdateAsync: Question details: {Details}", JsonSerializer.Serialize(question));
                
                // Verify the foreign keys exist
                bool attemptExists = await _context.UserQuizAttempts.AnyAsync(a => a.Id == question.UserQuizAttemptId);
                if (!attemptExists)
                {
                    _logger.LogError("CreateOrUpdateAsync: UserQuizAttempt with ID={AttemptId} does not exist", question.UserQuizAttemptId);
                    throw new InvalidOperationException($"UserQuizAttempt with ID={question.UserQuizAttemptId} does not exist");
                }
                
                // Check if question already exists for this attempt
                var entity = await _context.UserQuizAttemptQuestions
                    .FirstOrDefaultAsync(q => q.UserQuizAttemptId == question.UserQuizAttemptId && 
                                          q.UserQuizQuestionId == question.UserQuizQuestionId);

                string action;
                if (entity == null)
                {
                    action = "Created";
                    entity = MapToEntity(question);
                    _logger.LogInformation("CreateOrUpdateAsync: Creating new UserQuizAttemptQuestion record");
                    _context.UserQuizAttemptQuestions.Add(entity);
                }
                else
                {
                    action = "Updated";
                    _logger.LogInformation("CreateOrUpdateAsync: Updating existing UserQuizAttemptQuestion with ID={Id}", entity.Id);
                    entity.UserAnswer = question.UserAnswer;
                    entity.IsCorrect = question.IsCorrect;
                    entity.TimeTakenSeconds = question.TimeTakenSeconds;
                    _context.UserQuizAttemptQuestions.Update(entity);
                }

                // Save the entity to the database
                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation("CreateOrUpdateAsync: SaveChanges result: {SaveResult} records affected", saveResult);
                
                if (saveResult > 0)
                {
                    var result = MapToModel(entity);
                    _logger.LogInformation("CreateOrUpdateAsync: Successfully {Action} UserQuizAttemptQuestion with ID={Id}", 
                        action, entity.Id);
                    return result;
                }
                else
                {
                    _logger.LogWarning("CreateOrUpdateAsync: SaveChanges reported no records affected");
                    return MapToModel(entity); // Return anyway since it might be an update with no changes
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "CreateOrUpdateAsync: Database update error: {Message}, InnerException: {InnerException}", 
                    dbEx.Message, dbEx.InnerException?.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateOrUpdateAsync: Error creating/updating UserQuizAttemptQuestion: {Message}", ex.Message);
                throw;
            }
        }

        private Model.UserQuizAttemptQuestion MapToModel(Entities.UserQuizAttemptQuestion entity)
        {
            var model = new Model.UserQuizAttemptQuestion
            {
                Id = entity.Id,
                UserQuizAttemptId = entity.UserQuizAttemptId,
                UserQuizQuestionId = entity.UserQuizQuestionId,
                UserAnswer = entity.UserAnswer,
                IsCorrect = entity.IsCorrect,
                TimeTakenSeconds = entity.TimeTakenSeconds
            };
            
            _logger.LogDebug("MapToModel: Mapped entity {EntityId} to model", entity.Id);
            return model;
        }

        private Entities.UserQuizAttemptQuestion MapToEntity(Model.UserQuizAttemptQuestion model)
        {
            var entity = new Entities.UserQuizAttemptQuestion
            {
                Id = model.Id,
                UserQuizAttemptId = model.UserQuizAttemptId,
                UserQuizQuestionId = model.UserQuizQuestionId,
                UserAnswer = model.UserAnswer ?? string.Empty, // Ensure UserAnswer is not null
                IsCorrect = model.IsCorrect,
                TimeTakenSeconds = model.TimeTakenSeconds
            };
            
            _logger.LogDebug("MapToEntity: Mapped model {ModelId} to entity", model.Id);
            return entity;
        }
    }
}