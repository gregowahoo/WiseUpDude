using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories;
using Xunit;

namespace WiseUpDude.Tests
{
    public class QuizQuestionRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public QuizQuestionRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
                .Options;
        }

        private async Task<ApplicationDbContext> CreateDbContextAsync()
        {
            var context = new ApplicationDbContext(_options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        [Fact]
        public async Task AddAsync_ShouldAddQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestion = new QuizQuestion
            {
                Id = 1,
                Question = "Sample Question",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion);
            var result = await context.QuizQuestions.FindAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Sample Question", result.Question);
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizQuestions()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestion1 = new QuizQuestion
            {
                Id = 1,
                Question = "Sample Question 1",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation 1",
                QuizId = 1
            };
            var quizQuestion2 = new QuizQuestion
            {
                Id = 2,
                Question = "Sample Question 2",
                QuestionType = QuizQuestionType.TrueFalse,
                Answer = "True",
                Explanation = "Sample Explanation 2",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion1);
            await repository.AddAsync(quizQuestion2);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestion = new QuizQuestion
            {
                Id = 1,
                Question = "Sample Question",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion);
            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Sample Question", result.Question);
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestion = new QuizQuestion
            {
                Id = 1,
                Question = "Sample Question",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion);

            quizQuestion.Question = "Updated Question";
            await repository.UpdateAsync(quizQuestion);

            var result = await context.QuizQuestions.FindAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Updated Question", result.Question);
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestion = new QuizQuestion
            {
                Id = 1,
                Question = "Sample Question",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion);
            await repository.DeleteAsync(1);

            var result = await context.QuizQuestions.FindAsync(1);

            Assert.Null(result);
            await context.Database.EnsureDeletedAsync();
        }
    }
}
