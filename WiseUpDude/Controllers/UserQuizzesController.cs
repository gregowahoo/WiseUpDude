using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Services;

namespace WiseUpDude.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizzesController : ControllerBase
    {
        private readonly IUserQuizRepository<Quiz> _userQuizRepository;
        private readonly ILogger<UserQuizzesController> _logger;
        private readonly IUserIdLookupService _userIdLookupService;

        public UserQuizzesController(IUserQuizRepository<Quiz> userQuizRepository, ILogger<UserQuizzesController> logger, IUserIdLookupService userIdLookupService)
        {
            _userQuizRepository = userQuizRepository;
            _logger = logger;
            _userIdLookupService = userIdLookupService;
        }

        // GET: api/UserQuizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAll()
        {
            _logger.LogInformation("UserQuizzes/GetAll called");
            var quizzes = await _userQuizRepository.GetAllAsync();
            _logger.LogInformation("UserQuizzes/GetAll returning {Count} quizzes", quizzes?.Count() ?? 0);
            return Ok(quizzes);
        }

        // GET: api/UserQuizzes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetById(int id)
        {
            _logger.LogInformation("UserQuizzes/GetById called with id={Id}", id);
            try
            {
                var quiz = await _userQuizRepository.GetByIdAsync(id);
                if (quiz == null)
                {
                    _logger.LogWarning("UserQuizzes/GetById not found for id={Id}", id);
                    return NotFound();
                }
                _logger.LogInformation("UserQuizzes/GetById found quiz id={Id} with {QCount} questions", id, quiz.Questions?.Count ?? 0);
                return Ok(quiz);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("UserQuizzes/GetById repository threw KeyNotFound for id={Id}", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserQuizzes/GetById unexpected error for id={Id}");
                throw;
            }
        }

        // GET: api/UserQuizzes/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetByUserId(string userId)
        {
            _logger.LogInformation("UserQuizzes/GetByUserId called for userId={UserId}", userId);
            var quizzes = await _userQuizRepository.GetUserQuizzesAsync(userId);
            _logger.LogInformation("UserQuizzes/GetByUserId returning {Count} quizzes for userId={UserId}", quizzes?.Count() ?? 0, userId);
            return Ok(quizzes);
        }

        // POST: api/UserQuizzes
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Quiz quiz)
        {
            _logger.LogInformation("UserQuizzes/Create called. Name={Name} UserId={UserId}", quiz?.Name, quiz?.UserId);
            await _userQuizRepository.AddAsync(quiz);
            _logger.LogInformation("UserQuizzes/Create created quiz with Id={Id}", quiz.Id);
            return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
        }

        // GET: api/UserQuizzes/user/{userId}/recent?count=5
        [HttpGet("user/{userId}/recent")]
        public async Task<ActionResult<IEnumerable<RecentQuizDto>>> GetRecentUserQuizzes(string userId, [FromQuery] int count = 5)
        {
            _logger.LogInformation("UserQuizzes/GetRecentUserQuizzes called. userId={UserId} count={Count}", userId, count);
            var recent = await _userQuizRepository.GetRecentUserQuizzesAsync(userId, count);
            _logger.LogInformation("UserQuizzes/GetRecentUserQuizzes returning {Count} items for userId={UserId}", recent?.Count() ?? 0, userId);
            return Ok(recent);
        }

        // PUT: api/UserQuizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                _logger.LogWarning("UserQuizzes/Update bad request: route id {RouteId} != payload id {PayloadId}", id, quiz.Id);
                return BadRequest();
            }

            _logger.LogInformation("UserQuizzes/Update updating quiz id={Id}", id);
            await _userQuizRepository.UpdateAsync(quiz);
            return NoContent();
        }

        // PATCH: api/UserQuizzes/5/name
        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateQuizName(int id, [FromBody] string newName)
        {
            _logger.LogInformation("UserQuizzes/UpdateQuizName updating quiz id={Id} name={Name}", id, newName);
            await _userQuizRepository.UpdateQuizNameAsync(id, newName);
            return NoContent();
        }

        // PATCH: api/UserQuizzes/5/learnmode
        [HttpPut("{id}/learnmode")]
        public async Task<IActionResult> UpdateLearnMode(int id, [FromBody] bool learnMode)
        {
            _logger.LogInformation("UserQuizzes/UpdateLearnMode id={Id} learnMode={LearnMode}", id, learnMode);
            await _userQuizRepository.UpdateLearnModeAsync(id, learnMode);
            return NoContent();
        }

        // DELETE: api/UserQuizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("UserQuizzes/Delete id={Id}", id);
            await _userQuizRepository.DeleteAsync(id);
            return NoContent();
        }

        // POST: api/UserQuizzes/{id}/copy
        [HttpPost("{id}/copy")]
        public async Task<ActionResult<int>> CopyQuiz(int id)
        {
            _logger.LogInformation("UserQuizzes/CopyQuiz called for id={Id}", id);
            // Get the original quiz
            var originalQuiz = await _userQuizRepository.GetByIdAsync(id);
            if (originalQuiz == null)
            {
                _logger.LogWarning("UserQuizzes/CopyQuiz not found for id={Id}", id);
                return NotFound();
            }

            // Determine Special Picks owner UserId by email. Fallback to original owner if not found.
            const string specialOwnerEmail = "SpecialPicksOwner@wiseupdude.com";
            string? specialOwnerUserId = null;
            try
            {
                specialOwnerUserId = await _userIdLookupService.GetUserIdByEmailAsync(specialOwnerEmail);
                if (string.IsNullOrWhiteSpace(specialOwnerUserId))
                {
                    _logger.LogWarning("Special Picks owner not found by email {Email}. Falling back to original owner {OwnerId}", specialOwnerEmail, originalQuiz.UserId);
                }
                else
                {
                    _logger.LogInformation("Resolved Special Picks owner. Email={Email} UserId={UserId}", specialOwnerEmail, specialOwnerUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving Special Picks owner by email {Email}. Falling back to original owner {OwnerId}", specialOwnerEmail, originalQuiz.UserId);
            }

            var ownerUserId = string.IsNullOrWhiteSpace(specialOwnerUserId) ? originalQuiz.UserId : specialOwnerUserId;

            // Deep copy the quiz (excluding Id and related navigation properties)
            var copy = new Quiz
            {
                Name = originalQuiz.Name,
                UserName = originalQuiz.UserName,
                UserId = ownerUserId,
                Type = originalQuiz.Type,
                Topic = originalQuiz.Topic,
                Prompt = originalQuiz.Prompt,
                Description = originalQuiz.Description,
                Url = originalQuiz.Url,
                Difficulty = originalQuiz.Difficulty,
                TopicId = originalQuiz.TopicId,
                CreationDate = DateTime.UtcNow,
                LearnMode = false,
                Questions = originalQuiz.Questions?.Select(q => new QuizQuestion
                {
                    Question = q.Question,
                    QuestionType = q.QuestionType,
                    Options = q.Options != null ? new List<string>(q.Options) : new List<string>(),
                    Answer = q.Answer,
                    Explanation = q.Explanation,
                    Difficulty = q.Difficulty,
                    ContextSnippet = q.ContextSnippet,
                    Citation = q.Citation != null ? new List<CitationMeta>(q.Citation) : new List<CitationMeta>()
                }).ToList() ?? new List<QuizQuestion>()
            };

            // Use AddAsyncGetId to get the new quiz Id
            var newQuizId = await _userQuizRepository.AddAsyncGetId(copy);
            _logger.LogInformation("UserQuizzes/CopyQuiz created new quiz id={NewId} from id={OldId} ownedBy={OwnerId}", newQuizId, id, ownerUserId);
            return Ok(newQuizId);
        }
    }
}





