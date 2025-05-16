using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using WiseUpDude.Model;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizzesController : ControllerBase
    {
        private readonly IUserQuizRepository<Quiz> _userQuizRepository;

        public UserQuizzesController(IUserQuizRepository<Quiz> userQuizRepository)
        {
            _userQuizRepository = userQuizRepository;
        }

        // GET: api/UserQuizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAll()
        {
            var quizzes = await _userQuizRepository.GetAllAsync();
            return Ok(quizzes);
        }

        // GET: api/UserQuizzes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetById(int id)
        {
            try
            {
                var quiz = await _userQuizRepository.GetByIdAsync(id);
                return Ok(quiz);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/UserQuizzes/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetByUserId(string userId)
        {
            var quizzes = await _userQuizRepository.GetUserQuizzesAsync(userId);
            return Ok(quizzes);
        }

        // POST: api/UserQuizzes
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Quiz quiz)
        {
            await _userQuizRepository.AddAsync(quiz);
            return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
        }

        // GET: api/UserQuizzes/user/{userId}/recent?count=5
        [HttpGet("user/{userId}/recent")]
        public async Task<ActionResult<IEnumerable<RecentQuizDto>>> GetRecentUserQuizzes(string userId, [FromQuery] int count = 5)
        {
            var recent = await _userQuizRepository.GetRecentUserQuizzesAsync(userId, count);
            return Ok(recent);
        }

        // PUT: api/UserQuizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Quiz quiz)
        {
            if (id != quiz.Id)
                return BadRequest();

            await _userQuizRepository.UpdateAsync(quiz);
            return NoContent();
        }

        // PATCH: api/UserQuizzes/5/name
        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateQuizName(int id, [FromBody] string newName)
        {
            await _userQuizRepository.UpdateQuizNameAsync(id, newName);
            return NoContent();
        }

        // PATCH: api/UserQuizzes/5/learnmode
        [HttpPatch("{id}/learnmode")]
        public async Task<IActionResult> UpdateLearnMode(int id, [FromBody] bool learnMode)
        {
            await _userQuizRepository.UpdateLearnModeAsync(id, learnMode);
            return NoContent();
        }

        // DELETE: api/UserQuizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userQuizRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}



