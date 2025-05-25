using Microsoft.AspNetCore.Mvc;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserQuizQuestionsController : ControllerBase
    {
        private readonly IUserQuizQuestionRepository<QuizQuestion> _userQuizQuestionRepository;

        public UserQuizQuestionsController(IUserQuizQuestionRepository<QuizQuestion> userQuizQuestionRepository)
        {
            _userQuizQuestionRepository = userQuizQuestionRepository;
        }

        // PUT: api/UserQuizQuestion/{id}/useranswer
        [HttpPut("{id}/useranswer")]
        public async Task<IActionResult> UpdateUserAnswer(int id, [FromBody] string? userAnswer)
        {
            var question = await _userQuizQuestionRepository.GetByIdAsync(id);
            if (question == null)
                return NotFound();
            question.UserAnswer = userAnswer;
            await _userQuizQuestionRepository.UpdateAsync(question);
            return NoContent();
        }
    }
}
