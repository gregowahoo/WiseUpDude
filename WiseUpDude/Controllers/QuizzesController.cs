using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;

namespace WiseUpDude.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IRepository<Quiz> _quizRepository;
        public QuizzesController(IRepository<Quiz> quizRepository)
        {
            _quizRepository = quizRepository;
        }

        // GET: api/Quizzes
        [HttpGet]
        [OutputCache(Duration = 10)]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizzes()
        {
            var quizzes = await _quizRepository.GetAllAsync();
            return Ok(quizzes);
        }

        // GET: api/Quizzes/5
        [HttpGet("{id}")]
        [OutputCache(Duration = 10)]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            try
            {
                var quiz = await _quizRepository.GetByIdAsync(id);
                return Ok(quiz);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // PUT: api/Quizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return BadRequest();
            }

            try
            {
                await _quizRepository.UpdateAsync(quiz);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Quizzes
        [HttpPost]
        public async Task<ActionResult<Quiz>> PostQuiz(Quiz quiz)
        {
            await _quizRepository.AddAsync(quiz);
            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        // DELETE: api/Quizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                await _quizRepository.DeleteAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
