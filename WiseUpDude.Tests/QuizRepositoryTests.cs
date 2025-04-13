using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories;
using Xunit;

namespace WiseUpDude.Tests
{
    public class QuizRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public QuizRepositoryTests()
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
        public async Task AddAsync_ShouldAddQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository_Orig(context);
            var quiz = new Quiz_Orig
            {
                Id = 1,
                Name = "Sample Quiz_Orig",
                Questions = new List<QuizQuestion_Orig>
                {
                    new QuizQuestion_Orig
                    {
                        Id = 1,
                        Question = "Sample Question",
                        QuestionType = QuizQuestionType.MultipleChoice,
                        Options = new List<string> { "Option1", "Option2" },
                        Answer = "Option1",
                        Explanation = "Sample Explanation",
                        QuizId = 1
                    }
                }
            };

            await repository.AddAsync(quiz);
            var result = await context.Quizzes.FindAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Sample Quiz_Orig", result.Name);
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizzes()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository_Orig(context);
            var quiz1 = new Quiz_Orig
            {
                Id = 1,
                Name = "Sample Quiz_Orig 1",
                Questions = new List<QuizQuestion_Orig>
                {
                    new QuizQuestion_Orig
                    {
                        Id = 1,
                        Question = "Sample Question 1",
                        QuestionType = QuizQuestionType.MultipleChoice,
                        Options = new List<string> { "Option1", "Option2" },
                        Answer = "Option1",
                        Explanation = "Sample Explanation 1",
                        QuizId = 1
                    }
                }
            };
            var quiz2 = new Quiz_Orig
            {
                Id = 2,
                Name = "Sample Quiz_Orig 2",
                Questions = new List<QuizQuestion_Orig>
                {
                    new QuizQuestion_Orig
                    {
                        Id = 2,
                        Question = "Sample Question 2",
                        QuestionType = QuizQuestionType.TrueFalse,
                        Answer = "True",
                        Explanation = "Sample Explanation 2",
                        QuizId = 2
                    }
                }
            };

            await repository.AddAsync(quiz1);
            await repository.AddAsync(quiz2);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository_Orig(context);
            var quiz = new Quiz_Orig
            {
                Id = 1,
                Name = "Sample Quiz_Orig",
                Questions = new List<QuizQuestion_Orig>
                {
                    new QuizQuestion_Orig
                    {
                        Id = 1,
                        Question = "Sample Question",
                        QuestionType = QuizQuestionType.MultipleChoice,
                        Options = new List<string> { "Option1", "Option2" },
                        Answer = "Option1",
                        Explanation = "Sample Explanation",
                        QuizId = 1
                    }
                }
            };

            await repository.AddAsync(quiz);
            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Sample Quiz_Orig", result.Name);
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository_Orig(context);
            var quiz = new Quiz_Orig
            {
                Id = 1,
                Name = "Sample Quiz_Orig",
                Questions = new List<QuizQuestion_Orig>
                {
                    new QuizQuestion_Orig
                    {
                        Id = 1,
                        Question = "Sample Question",
                        QuestionType = QuizQuestionType.MultipleChoice,
                        Options = new List<string> { "Option1", "Option2" },
                        Answer = "Option1",
                        Explanation = "Sample Explanation",
                        QuizId = 1
                    }
                }
            };

            await repository.AddAsync(quiz);

            quiz.Name = "Updated Quiz_Orig";
            await repository.UpdateAsync(quiz);

            var result = await context.Quizzes.FindAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Updated Quiz_Orig", result.Name);
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository_Orig(context);
            var quiz = new Quiz_Orig
            {
                Id = 1,
                Name = "Sample Quiz_Orig",
                Questions = new List<QuizQuestion_Orig>
                {
                    new QuizQuestion_Orig
                    {
                        Id = 1,
                        Question = "Sample Question",
                        QuestionType = QuizQuestionType.MultipleChoice,
                        Options = new List<string> { "Option1", "Option2" },
                        Answer = "Option1",
                        Explanation = "Sample Explanation",
                        QuizId = 1
                    }
                }
            };

            await repository.AddAsync(quiz);
            await repository.DeleteAsync(1);

            var result = await context.Quizzes.FindAsync(1);

            Assert.Null(result);
            await context.Database.EnsureDeletedAsync();
        }
    }
}
