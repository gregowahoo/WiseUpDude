using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories
{
    public class UserQuizQuestionRepository :  IUserQuizQuestionRepository<WiseUpDude.Model.QuizQuestion>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UserQuizQuestionRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<WiseUpDude.Model.QuizQuestion>> GetAllAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizQuestions = await context.UserQuizQuestions.ToListAsync();
            return userQuizQuestions.Select(uqq => new WiseUpDude.Model.QuizQuestion
            {
                Id = uqq.Id,
                Question = uqq.Question,
                QuestionType = (WiseUpDude.Model.QuizQuestionType)uqq.QuestionType,
                Options = string.IsNullOrEmpty(uqq.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(uqq.OptionsJson),
                Answer = uqq.Answer,
                Explanation = uqq.Explanation,
                UserAnswer = uqq.UserAnswer,
                QuizId = uqq.QuizId
            });
        }

        public async Task<WiseUpDude.Model.QuizQuestion> GetByIdAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizQuestion = await context.UserQuizQuestions.FirstOrDefaultAsync(uqq => uqq.Id == id);
            if (userQuizQuestion == null)
                throw new KeyNotFoundException($"UserQuizQuestion with Id {id} not found.");

            return new WiseUpDude.Model.QuizQuestion
            {
                Id = userQuizQuestion.Id,
                Question = userQuizQuestion.Question,
                QuestionType = (WiseUpDude.Model.QuizQuestionType)userQuizQuestion.QuestionType,
                Options = string.IsNullOrEmpty(userQuizQuestion.OptionsJson) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(userQuizQuestion.OptionsJson),
                Answer = userQuizQuestion.Answer,
                Explanation = userQuizQuestion.Explanation,
                UserAnswer = userQuizQuestion.UserAnswer,
                QuizId = userQuizQuestion.QuizId
            };
        }

        public async Task AddAsync(WiseUpDude.Model.QuizQuestion quizQuestion)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuiz = await context.UserQuizzes.FirstOrDefaultAsync(uq => uq.Id == quizQuestion.QuizId);
            if (userQuiz == null)
                throw new KeyNotFoundException($"UserQuiz with Id {quizQuestion.QuizId} not found.");

            var userQuizQuestion = new UserQuizQuestion
            {
                Question = quizQuestion.Question,
                QuestionType = (UserQuizQuestionType)quizQuestion.QuestionType,
                OptionsJson = quizQuestion.Options == null ? null : System.Text.Json.JsonSerializer.Serialize(quizQuestion.Options),
                Answer = quizQuestion.Answer,
                Explanation = quizQuestion.Explanation,
                UserAnswer = quizQuestion.UserAnswer,
                QuizId = quizQuestion.QuizId,
                Quiz = userQuiz // Set the Quiz property
            };

            context.UserQuizQuestions.Add(userQuizQuestion);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WiseUpDude.Model.QuizQuestion quizQuestion)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizQuestion = await context.UserQuizQuestions.FirstOrDefaultAsync(uqq => uqq.Id == quizQuestion.Id);
            if (userQuizQuestion == null)
                throw new KeyNotFoundException($"UserQuizQuestion with Id {quizQuestion.Id} not found.");

            userQuizQuestion.Question = quizQuestion.Question;
            userQuizQuestion.QuestionType = (UserQuizQuestionType)quizQuestion.QuestionType;
            userQuizQuestion.OptionsJson = quizQuestion.Options != null ? System.Text.Json.JsonSerializer.Serialize(quizQuestion.Options) : null;
            userQuizQuestion.Answer = quizQuestion.Answer;
            userQuizQuestion.Explanation = quizQuestion.Explanation;
            userQuizQuestion.UserAnswer = quizQuestion.UserAnswer;

            context.Entry(userQuizQuestion).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userQuizQuestion = await context.UserQuizQuestions.FindAsync(id);
            if (userQuizQuestion != null)
            {
                context.UserQuizQuestions.Remove(userQuizQuestion);
                await context.SaveChangesAsync();
            }
        }

        public async Task ClearUserAnswersByQuizIdAsync(int quizId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var questions = await context.UserQuizQuestions.Where(q => q.QuizId == quizId).ToListAsync();
            foreach (var question in questions)
            {
                question.UserAnswer = null;
            }
            await context.SaveChangesAsync();
        }
    }
}

