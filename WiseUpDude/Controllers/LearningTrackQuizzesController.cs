using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningTrackQuizzesController : ControllerBase
    {
        private readonly ILearningTrackQuizRepository _quizRepository;
        public LearningTrackQuizzesController(ILearningTrackQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        // GET: api/LearningTrackQuizzes
        [HttpGet]
        [OutputCache(Duration = 10)]
        public async Task<ActionResult<IEnumerable<LearningTrackQuiz>>> GetQuizzes()
        {
            var quizzes = await _quizRepository.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        // GET: api/LearningTrackQuizzes/5
        [HttpGet("{id}")]
        [OutputCache(Duration = 10)]
        public async Task<ActionResult<LearningTrackQuiz>> GetQuiz(int id)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(id);
            if (quiz == null)
                return NotFound();
            return Ok(quiz);
        }

        // PUT: api/LearningTrackQuizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, LearningTrackQuiz quiz)
        {
            if (id != quiz.Id)
                return BadRequest();

            await _quizRepository.UpdateQuizAsync(quiz);
            return NoContent();
        }

        // POST: api/LearningTrackQuizzes
        [HttpPost]
        public async Task<ActionResult<LearningTrackQuiz>> PostQuiz(LearningTrackQuiz quiz)
        {
            await _quizRepository.AddQuizAsync(quiz);
            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        // DELETE: api/LearningTrackQuizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            await _quizRepository.DeleteQuizAsync(id);
            return NoContent();
        }
    }
}