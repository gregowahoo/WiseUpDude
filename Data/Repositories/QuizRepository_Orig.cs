using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Model;
using WiseUpDude.Data;

namespace WiseUpDude.Data.Repositories
{
    public class QuizRepository_Orig : IRepository<Quiz_Orig>
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository_Orig(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quiz_Orig>> GetAllAsync()
        {
            return await _context.Quizzes.Include(q => q.Questions).ToListAsync();
        }

        public async Task<Quiz_Orig> GetByIdAsync(int id)
        {
            return await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task AddAsync(Quiz_Orig entity)
        {
            _context.Quizzes.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Quiz_Orig entity)
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
