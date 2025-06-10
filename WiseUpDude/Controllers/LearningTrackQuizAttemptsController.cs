using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LearningTrackQuizAttemptsController : ControllerBase
    {
        private readonly ILearningTrackQuizAttemptRepository _attemptRepository;
        private readonly ILearningTrackQuizAttemptQuestionRepository _questionRepository;
        private readonly ILogger<LearningTrackQuizAttemptsController> _logger;

        public LearningTrackQuizAttemptsController(
            ILearningTrackQuizAttemptRepository attemptRepository,
            ILearningTrackQuizAttemptQuestionRepository questionRepository,
            ILogger<LearningTrackQuizAttemptsController> logger)
        {
            _attemptRepository = attemptRepository;
            _questionRepository = questionRepository;
            _logger = logger;
        }

        // --- LearningTrackQuizAttempt CRUD ---
        [HttpGet("{id}")]
        public async Task<ActionResult<LearningTrackQuizAttempt>> GetAttemptById(int id)
        {
            var attempt = await _attemptRepository.GetByIdAsync(id);
            if (attempt == null) return NotFound();
            return Ok(attempt);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearningTrackQuizAttempt>>> GetAllAttempts()
        {
            var attempts = await _attemptRepository.GetAllAsync();
            return Ok(attempts);
        }

        [HttpPost]
        public async Task<ActionResult<LearningTrackQuizAttempt>> CreateAttempt([FromBody] LearningTrackQuizAttempt attempt)
        {
            await _attemptRepository.AddAsync(attempt);
            return CreatedAtAction(nameof(GetAttemptById), new { id = attempt.Id }, attempt);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttempt(int id, [FromBody] LearningTrackQuizAttempt attempt)
        {
            if (id != attempt.Id) return BadRequest();
            await _attemptRepository.UpdateAsync(attempt);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttempt(int id)
        {
            await _attemptRepository.DeleteAsync(id);
            return NoContent();
        }

        // --- LearningTrackQuizAttemptQuestion CRUD ---
        [HttpGet("question/{id}")]
        public async Task<ActionResult<LearningTrackQuizAttemptQuestion>> GetQuestionById(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(question);
        }

        [HttpPost("question")]
        public async Task<ActionResult<LearningTrackQuizAttemptQuestion>> CreateQuestion([FromBody] LearningTrackQuizAttemptQuestion question)
        {
            await _questionRepository.AddAsync(question);
            return CreatedAtAction(nameof(GetQuestionById), new { id = question.Id }, question);
        }

        [HttpPut("question/{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] LearningTrackQuizAttemptQuestion question)
        {
            if (id != question.Id) return BadRequest();
            await _questionRepository.UpdateAsync(question);
            return NoContent();
        }

        [HttpDelete("question/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _questionRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
