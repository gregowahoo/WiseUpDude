using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class QuizQuestionRepository : IRepository<QuizQuestion>
    {
        private readonly ApplicationDbContext _context;

        public QuizQuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuizQuestion>> GetAllAsync()
        {
            return await _context.QuizQuestions.ToListAsync();
        }

        public async Task<QuizQuestion> GetByIdAsync(int id)
        {
            return await _context.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id) ?? throw new KeyNotFoundException($"QuizQuestion with Id {id} not found.");
        }

        public async Task AddAsync(QuizQuestion entity)
        {
            _context.QuizQuestions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(QuizQuestion entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
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
