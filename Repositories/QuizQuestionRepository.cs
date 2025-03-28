using WiseUpDude.Model;
using WiseUpDude.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WiseUpDude.Repositories
{
    public class QuizQuestionRepository : IQuizQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizQuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuizQuestion> GetQuizQuestionByIdAsync(int id)
        {
            return await _context.QuizQuestions.FindAsync(id);
        }

        public async Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync()
        {
            return await _context.QuizQuestions.ToListAsync();
        }

        public async Task AddQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _context.QuizQuestions.Add(quizQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuizQuestionAsync(QuizQuestion quizQuestion)
        {
            _context.QuizQuestions.Update(quizQuestion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizQuestionAsync(int id)
        {
            var quizQuestion = await _context.QuizQuestions.FindAsync(id);
            if (quizQuestion != null)
            {
                _context.QuizQuestions.Remove(quizQuestion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
