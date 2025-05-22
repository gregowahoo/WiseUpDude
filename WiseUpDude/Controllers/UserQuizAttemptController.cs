using Microsoft.AspNetCore.Mvc;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizAttemptController : ControllerBase
    {
        private readonly IUserQuizAttemptRepository<UserQuizAttempt> _repository;
        public UserQuizAttemptController(IUserQuizAttemptRepository<UserQuizAttempt> repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserQuizAttempt>> GetById(int id)
        {
            var attempt = await _repository.GetByIdAsync(id);
            if (attempt == null) return NotFound();
            return Ok(attempt);
        }

        [HttpGet("byUserQuiz/{userQuizId}")]
        public async Task<ActionResult<IEnumerable<UserQuizAttempt>>> GetByUserQuizId(int userQuizId)
        {
            var attempts = await _repository.GetByUserQuizIdAsync(userQuizId);
            return Ok(attempts);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserQuizAttempt attempt)
        {
            await _repository.AddAsync(attempt);
            return CreatedAtAction(nameof(GetById), new { id = attempt.Id }, attempt);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserQuizAttempt attempt)
        {
            if (id != attempt.Id) return BadRequest();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _repository.UpdateAsync(attempt);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}