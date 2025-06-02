using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;

namespace WiseUpDude.Data.Repositories
{
    public class LearningTrackQuizRepository : ILearningTrackQuizRepository
    {
        private readonly ApplicationDbContext _context;
        public LearningTrackQuizRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<LearningTrackQuiz>> GetAllQuizzesAsync() => await _context.LearningTrackQuizzes.Include(q => q.Questions).ToListAsync();
        public async Task<LearningTrackQuiz?> GetQuizByIdAsync(int id) => await _context.LearningTrackQuizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
        public async Task AddQuizAsync(LearningTrackQuiz quiz) { _context.LearningTrackQuizzes.Add(quiz); await _context.SaveChangesAsync(); }
        public async Task UpdateQuizAsync(LearningTrackQuiz quiz) { _context.LearningTrackQuizzes.Update(quiz); await _context.SaveChangesAsync(); }
        public async Task DeleteQuizAsync(int id) { var quiz = await _context.LearningTrackQuizzes.FindAsync(id); if (quiz != null) { _context.LearningTrackQuizzes.Remove(quiz); await _context.SaveChangesAsync(); } }

        public async Task<IEnumerable<LearningTrackQuizQuestion>> GetQuestionsByQuizIdAsync(int quizId) => await _context.LearningTrackQuizQuestions.Where(q => q.LearningTrackQuizId == quizId).ToListAsync();
        public async Task<LearningTrackQuizQuestion?> GetQuestionByIdAsync(int id) => await _context.LearningTrackQuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
        public async Task AddQuestionAsync(LearningTrackQuizQuestion question) { _context.LearningTrackQuizQuestions.Add(question); await _context.SaveChangesAsync(); }
        public async Task UpdateQuestionAsync(LearningTrackQuizQuestion question) { _context.LearningTrackQuizQuestions.Update(question); await _context.SaveChangesAsync(); }
        public async Task DeleteQuestionAsync(int id) { var question = await _context.LearningTrackQuizQuestions.FindAsync(id); if (question != null) { _context.LearningTrackQuizQuestions.Remove(question); await _context.SaveChangesAsync(); } }
    }
}
