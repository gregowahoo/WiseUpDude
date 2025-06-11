using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using DataEntity = WiseUpDude.Data.Entities;
using System.Linq;

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
            return Ok(EntityToModel(attempt));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LearningTrackQuizAttempt>>> GetAllAttempts()
        {
            var attempts = await _attemptRepository.GetAllAsync();
            return Ok(attempts.Select(EntityToModel));
        }

        [HttpPost]
        public async Task<ActionResult<LearningTrackQuizAttempt>> CreateAttempt([FromBody] LearningTrackQuizAttemptCreateDto dto)
        {
            var attempt = DtoToEntity(dto);
            await _attemptRepository.AddAsync(attempt);
            return CreatedAtAction(nameof(GetAttemptById), new { id = attempt.Id }, EntityToModel(attempt));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttempt(int id, [FromBody] LearningTrackQuizAttempt model)
        {
            if (id != model.Id) return BadRequest();
            var entity = ModelToEntity(model);
            await _attemptRepository.UpdateAsync(entity);
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
            return Ok(EntityToModel(question));
        }

        [HttpPost("question")]
        public async Task<ActionResult<LearningTrackQuizAttemptQuestion>> CreateQuestion([FromBody] LearningTrackQuizAttemptQuestion model)
        {
            var entity = ModelToEntity(model);
            await _questionRepository.AddAsync(entity);
            return CreatedAtAction(nameof(GetQuestionById), new { id = entity.Id }, EntityToModel(entity));
        }

        [HttpPut("question/{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] LearningTrackQuizAttemptQuestion model)
        {
            if (id != model.Id) return BadRequest();
            var entity = ModelToEntity(model);
            await _questionRepository.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("question/{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _questionRepository.DeleteAsync(id);
            return NoContent();
        }

        // --- Mapping helpers ---
        private static LearningTrackQuizAttempt EntityToModel(DataEntity.LearningTrackQuizAttempt entity) => new LearningTrackQuizAttempt
        {
            Id = entity.Id,
            LearningTrackQuizId = entity.LearningTrackQuizId,
            AttemptDate = entity.AttemptDate,
            Score = entity.Score,
            Duration = entity.Duration,
            IsComplete = entity.IsComplete,
            AttemptQuestions = entity.AttemptQuestions?.Select(EntityToModel).ToList() ?? new()
        };

        private static DataEntity.LearningTrackQuizAttempt DtoToEntity(LearningTrackQuizAttemptCreateDto dto) => new DataEntity.LearningTrackQuizAttempt
        {
            LearningTrackQuizId = dto.LearningTrackQuizId,
            AttemptDate = dto.AttemptDate,
            Score = dto.Score,
            Duration = dto.Duration,
            IsComplete = dto.IsComplete,
            AttemptQuestions = dto.AttemptQuestions?.Select(DtoToEntity).ToList() ?? new()
        };

        private static DataEntity.LearningTrackQuizAttempt ModelToEntity(LearningTrackQuizAttempt model) => new DataEntity.LearningTrackQuizAttempt
        {
            Id = model.Id,
            LearningTrackQuizId = model.LearningTrackQuizId,
            AttemptDate = model.AttemptDate,
            Score = model.Score,
            Duration = model.Duration,
            IsComplete = model.IsComplete,
            AttemptQuestions = model.AttemptQuestions?.Select(ModelToEntity).ToList() ?? new()
        };

        private static LearningTrackQuizAttemptQuestion EntityToModel(DataEntity.LearningTrackQuizAttemptQuestion entity) => new LearningTrackQuizAttemptQuestion
        {
            Id = entity.Id,
            LearningTrackAttemptId = entity.LearningTrackAttemptId,
            LearningTrackQuestionId = entity.LearningTrackQuestionId,
            UserAnswer = entity.UserAnswer,
            IsCorrect = entity.IsCorrect,
            TimeTakenSeconds = entity.TimeTakenSeconds
        };

        private static DataEntity.LearningTrackQuizAttemptQuestion DtoToEntity(LearningTrackQuizAttemptQuestionCreateDto dto) => new DataEntity.LearningTrackQuizAttemptQuestion
        {
            LearningTrackQuestionId = dto.LearningTrackQuestionId,
            UserAnswer = dto.UserAnswer,
            IsCorrect = dto.IsCorrect,
            TimeTakenSeconds = dto.TimeTakenSeconds ?? 0
        };

        private static DataEntity.LearningTrackQuizAttemptQuestion ModelToEntity(LearningTrackQuizAttemptQuestion model) => new DataEntity.LearningTrackQuizAttemptQuestion
        {
            Id = model.Id,
            LearningTrackAttemptId = model.LearningTrackAttemptId,
            LearningTrackQuestionId = model.LearningTrackQuestionId,
            UserAnswer = model.UserAnswer,
            IsCorrect = model.IsCorrect,
            TimeTakenSeconds = model.TimeTakenSeconds ?? 0
        };
    }
}
