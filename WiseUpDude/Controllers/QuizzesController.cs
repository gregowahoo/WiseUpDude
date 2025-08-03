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

        // GET: api/Quizzes/QuizOfTheDay
        [HttpGet("QuizOfTheDay")]
        [OutputCache(Duration = 60)]
        public async Task<ActionResult<Quiz>> GetCurrentQuizOfTheDay()
        {
            var today = DateTime.Today;
            var quizzes = await _quizRepository.GetAllAsync();
            var todaysQuiz = quizzes.FirstOrDefault(q => q.IsQuizOfTheDay && 
                q.QuizOfTheDayDate.HasValue && 
                q.QuizOfTheDayDate.Value.Date == today);
            
            if (todaysQuiz == null)
            {
                return NotFound("No Quiz of the Day set for today");
            }
            
            return Ok(todaysQuiz);
        }

        // GET: api/Quizzes/QuizOfTheDay/All
        [HttpGet("QuizOfTheDay/All")]
        [OutputCache(Duration = 300)]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAllQuizzesOfTheDay()
        {
            var quizzes = await _quizRepository.GetAllAsync();
            var quizzesOfTheDay = quizzes.Where(q => q.IsQuizOfTheDay && q.QuizOfTheDayDate.HasValue)
                                        .OrderByDescending(q => q.QuizOfTheDayDate)
                                        .ToList();
            
            return Ok(quizzesOfTheDay);
        }

        // POST: api/Quizzes/{id}/SetQuizOfTheDay
        [HttpPost("{id}/SetQuizOfTheDay")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetQuizOfTheDay(int id, [FromBody] DateTime? date = null)
        {
            try
            {
                var quiz = await _quizRepository.GetByIdAsync(id);
                quiz.IsQuizOfTheDay = true;
                quiz.QuizOfTheDayDate = date ?? DateTime.Today;
                
                await _quizRepository.UpdateAsync(quiz);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/Quizzes/{id}/RemoveQuizOfTheDay
        [HttpDelete("{id}/RemoveQuizOfTheDay")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveQuizOfTheDay(int id)
        {
            try
            {
                var quiz = await _quizRepository.GetByIdAsync(id);
                quiz.IsQuizOfTheDay = false;
                quiz.QuizOfTheDayDate = null;
                
                await _quizRepository.UpdateAsync(quiz);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
