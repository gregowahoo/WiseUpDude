using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizAttemptQuestionController : ControllerBase
    {
        private readonly IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion> _repository;
        private readonly ILogger<UserQuizAttemptQuestionController> _logger;

        public UserQuizAttemptQuestionController(
            IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion> repository,
            ILogger<UserQuizAttemptQuestionController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserQuizAttemptQuestion>> GetById(int id)
        {
            _logger.LogInformation("GetById: Fetching question with id={Id}", id);
            
            try
            {
                var question = await _repository.GetByIdAsync(id);
                if (question == null)
                {
                    _logger.LogWarning("GetById: Question with id={Id} not found", id);
                    return NotFound();
                }
                
                _logger.LogInformation("GetById: Successfully retrieved question with id={Id}", id);
                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById: Error retrieving question with id={Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An error occurred while retrieving the question");
            }
        }

        [HttpGet("byAttempt/{attemptId}")]
        public async Task<ActionResult<IEnumerable<UserQuizAttemptQuestion>>> GetByAttemptId(int attemptId)
        {
            _logger.LogInformation("GetByAttemptId: Fetching questions for attemptId={AttemptId}", attemptId);
            
            try
            {
                var questions = await _repository.GetByAttemptIdAsync(attemptId);
                _logger.LogInformation("GetByAttemptId: Retrieved {Count} questions for attemptId={AttemptId}", 
                    questions?.Count() ?? 0, attemptId);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByAttemptId: Error retrieving questions for attemptId={AttemptId}: {Message}", 
                    attemptId, ex.Message);
                return StatusCode(500, "An error occurred while retrieving questions");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserQuizAttemptQuestion question)
        {
            _logger.LogInformation("Create: Creating new question for attemptId={AttemptId}, questionId={QuestionId}", 
                question.UserQuizAttemptId, question.UserQuizQuestionId);
            
            try
            {
                _logger.LogDebug("Create: Request payload: {Payload}", JsonSerializer.Serialize(question));
                await _repository.AddAsync(question);
                
                _logger.LogInformation("Create: Successfully created question with id={Id}", question.Id);
                return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create: Error creating question: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while creating the question");
            }
        }

        [HttpPost("CreateOrUpdate")]
        public async Task<ActionResult<UserQuizAttemptQuestion>> CreateOrUpdate([FromBody] UserQuizAttemptQuestion question)
        {
            _logger.LogInformation("CreateOrUpdate: Creating/updating question for attemptId={AttemptId}, questionId={QuestionId}", 
                question.UserQuizAttemptId, question.UserQuizQuestionId);
            
            try
            {
                _logger.LogDebug("CreateOrUpdate: Request payload: {Payload}", JsonSerializer.Serialize(question));
                var result = await _repository.CreateOrUpdateAsync(question);
                
                if (result != null)
                {
                    _logger.LogInformation("CreateOrUpdate: Successfully created/updated question with id={Id}", result.Id);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("CreateOrUpdate: Repository returned null result");
                    return StatusCode(500, "Operation did not return a valid result");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateOrUpdate: Error creating/updating question: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while processing the question");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserQuizAttemptQuestion question)
        {
            _logger.LogInformation("Update: Updating question with id={Id}", id);
            
            try
            {
                if (id != question.Id)
                {
                    _logger.LogWarning("Update: Id mismatch - URL id={UrlId}, question.Id={QuestionId}", id, question.Id);
                    return BadRequest("Id in URL does not match Id in request body");
                }
                
                _logger.LogDebug("Update: Request payload: {Payload}", JsonSerializer.Serialize(question));
                await _repository.UpdateAsync(question);
                
                _logger.LogInformation("Update: Successfully updated question with id={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update: Error updating question with id={Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An error occurred while updating the question");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete: Deleting question with id={Id}", id);
            
            try
            {
                await _repository.DeleteAsync(id);
                _logger.LogInformation("Delete: Successfully deleted question with id={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete: Error deleting question with id={Id}: {Message}", id, ex.Message);
                return StatusCode(500, "An error occurred while deleting the question");
            }
        }
    }
}