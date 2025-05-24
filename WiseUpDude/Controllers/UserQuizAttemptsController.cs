using Microsoft.AspNetCore.Mvc;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizAttemptsController : ControllerBase
    {
        private readonly IUserQuizAttemptRepository<UserQuizAttempt> _userQuizAttemptRepository;
        private readonly ILogger<UserQuizAttemptsController> _logger;

        public UserQuizAttemptsController(IUserQuizAttemptRepository<UserQuizAttempt> userQuizAttemptRepository, ILogger<UserQuizAttemptsController> logger)
        {
            _userQuizAttemptRepository = userQuizAttemptRepository;
            _logger = logger;
        }

        // GET: api/UserQuizAttempts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserQuizAttempt>> GetById(int id)
        {
            _logger.LogInformation("Getting UserQuizAttempt by Id: {Id}", id);
            var attempt = await _userQuizAttemptRepository.GetByIdAsync(id);
            if (attempt == null)
            {
                _logger.LogWarning("UserQuizAttempt with Id {Id} not found.", id);
                return NotFound();
            }
            return Ok(attempt);
        }

        // GET: api/UserQuizAttempts/byUserQuiz/{userQuizId}
        [HttpGet("byUserQuiz/{userQuizId}")]
        public async Task<ActionResult<IEnumerable<UserQuizAttempt>>> GetByUserQuizId(int userQuizId)
        {
            _logger.LogInformation("Getting UserQuizAttempts by UserQuizId: {UserQuizId}", userQuizId);
            var attempts = await _userQuizAttemptRepository.GetByUserQuizIdAsync(userQuizId);
            return Ok(attempts);
        }

        // POST: api/UserQuizAttempts
        [HttpPost]
        public async Task<ActionResult<UserQuizAttempt>> Create([FromBody] UserQuizAttempt attempt)
        {
            _logger.LogInformation("Creating new UserQuizAttempt for UserQuizId: {UserQuizId}", attempt.UserQuizId);
            var created = await _userQuizAttemptRepository.AddAsync(attempt);
            _logger.LogInformation("Created UserQuizAttempt with Id: {Id}", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/UserQuizAttempts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserQuizAttempt attempt)
        {
            if (id != attempt.Id)
            {
                _logger.LogWarning("Update failed: route id {Id} does not match attempt id {AttemptId}", id, attempt.Id);
                return BadRequest();
            }
            _logger.LogInformation("Updating UserQuizAttempt with Id: {Id}", id);
            await _userQuizAttemptRepository.UpdateAsync(attempt);
            _logger.LogInformation("Updated UserQuizAttempt with Id: {Id}", id);
            return NoContent();
        }

        // DELETE: api/UserQuizAttempts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting UserQuizAttempt with Id: {Id}", id);
            await _userQuizAttemptRepository.DeleteAsync(id);
            _logger.LogInformation("Deleted UserQuizAttempt with Id: {Id}", id);
            return NoContent();
        }
    }
}
