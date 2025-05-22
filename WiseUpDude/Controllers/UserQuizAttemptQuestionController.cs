using Microsoft.AspNetCore.Mvc;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizAttemptQuestionController : ControllerBase
    {
        private readonly IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion> _repository;
        public UserQuizAttemptQuestionController(IUserQuizAttemptQuestionRepository<UserQuizAttemptQuestion> repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserQuizAttemptQuestion>> GetById(int id)
        {
            var question = await _repository.GetByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(question);
        }

        [HttpGet("byAttempt/{attemptId}")]
        public async Task<ActionResult<IEnumerable<UserQuizAttemptQuestion>>> GetByAttemptId(int attemptId)
        {
            var questions = await _repository.GetByAttemptIdAsync(attemptId);
            return Ok(questions);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UserQuizAttemptQuestion question)
        {
            await _repository.AddAsync(question);
            return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserQuizAttemptQuestion question)
        {
            if (id != question.Id) return BadRequest();
            await _repository.UpdateAsync(question);
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