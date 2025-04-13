using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class QuizRepository : IRepository<Model.Quiz>
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Model.Quiz>> GetAllAsync()
        {
            var entities = await _context.Quizzes.Include(q => q.Questions).ToListAsync();
            return entities.Select(e => new Model.Quiz
            {
                Id = e.Id,
                Name = e.Name,
                Questions = e.Questions.Select(q => new Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer
                }).ToList()
            });
        }

        public async Task<Model.Quiz> GetByIdAsync(int id)
        {
            var entity = await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {id} not found.");

            return new Model.Quiz
            {
                Id = entity.Id,
                Name = entity.Name,
                Questions = entity.Questions.Select(q => new Model.QuizQuestion
                {
                    Id = q.Id,
                    Question = q.Question,
                    Answer = q.Answer
                }).ToList()
            };
        }

        //public async Task AddAsync(Model.Quiz model)
        //{
        //    var entity = new Data.Entities.Quiz
        //    {
        //        Name = model.Name,
        //        Questions = model.Questions.Select(q => new Data.Entities.QuizQuestion
        //        {
        //            Question = q.Question,
        //            Answer = q.Answer
        //        }).ToList()
        //    };

        //    _context.Quizzes.Add(entity);
        //    await _context.SaveChangesAsync();
        //}

        //public async Task AddAsync(Model.Quiz model)
        //{
        //    var entity = new Entities.Quiz
        //    {
        //        Name = model.Name,
        //        Questions = model.Questions.Select(q => new Entities.QuizQuestion
        //        {
        //            Question = q.Question,
        //            QuestionType = (Entities.QuizQuestionType)q.QuestionType,
        //            OptionsJson = q.Options != null ? System.Text.Json.JsonSerializer.Serialize(q.Options) : null,
        //            Answer = q.Answer,
        //            Explanation = q.Explanation
        //        }).ToList()
        //    };

        //    await _context.Quizzes.AddAsync(entity); // Add entity to the DbContext
        //    await _context.SaveChangesAsync();       // Save changes to the database

        //    // Map the generated Id back to the model
        //    model.Id = entity.Id;
        //    //model.UserName = entity.User?.UserName ?? string.Empty; // Use null-coalescing operator to handle null values
        //}

        public async Task AddAsync(Model.Quiz model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var entity = new Entities.Quiz
            {
                Name = model.Name,
                Questions = model.Questions.Select(q => new Entities.QuizQuestion
                {
                    Question = q.Question,
                    QuestionType = (Entities.QuizQuestionType)q.QuestionType,
                    OptionsJson = q.Options != null ? System.Text.Json.JsonSerializer.Serialize(q.Options) : null,
                    Answer = q.Answer,
                    Explanation = q.Explanation
                }).ToList(),
                User = user // Assign the user
            };

            await _context.Quizzes.AddAsync(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            model.User = user; // Map back the user if needed
        }

        public async Task UpdateAsync(Model.Quiz model)
        {
            var entity = await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == model.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Quiz with Id {model.Id} not found.");

            entity.Name = model.Name;
            entity.Questions = model.Questions.Select(q => new Data.Entities.QuizQuestion
            {
                Id = q.Id,
                Question = q.Question,
                Answer = q.Answer
            }).ToList();

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Quizzes.FindAsync(id);
            if (entity != null)
            {
                _context.Quizzes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}

