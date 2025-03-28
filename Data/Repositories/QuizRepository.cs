using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Model;
using WiseUpDude.Data;

namespace GarbageIn.Data.Repositories
{
    public class QuizRepository : IRepository<Quiz>
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quiz>> GetAllAsync()
        {
            return await _context.Quizzes.Include(q => q.Questions).ToListAsync();
        }

        public async Task<Quiz> GetByIdAsync(int id)
        {
            return await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task AddAsync(Quiz entity)
        {
            _context.Quizzes.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Quiz entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }
    }
}
