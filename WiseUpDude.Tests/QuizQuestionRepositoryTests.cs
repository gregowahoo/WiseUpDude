using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiseUpDude.Data;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;
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
                Question = "Sample Question",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation"
            };

            await repository.AddAsync(quizQuestion);
            var result = await context.QuizQuestions.FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.Equal("Sample Question", result.Question);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizQuestions()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "Question 1",
                    QuestionType = QuizQuestionType.MultipleChoice,
                    Options = new List<string> { "Option1", "Option2" },
                    Answer = "Option1",
                    Explanation = "Explanation 1"
                },
                new QuizQuestion
                {
                    Question = "Question 2",
                    QuestionType = QuizQuestionType.TrueFalse,
                    Answer = "True",
                    Explanation = "Explanation 2"
                }
            };

            foreach (var quizQuestion in quizQuestions)
            {
                await repository.AddAsync(quizQuestion);
            }

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);

            // Add a QuizQuestion with a specific ID
            var quizQuestion = new QuizQuestion
            {
                Id = 1, // Assign a valid ID
                Question = "Sample Question",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion);

            // Attempt to retrieve the QuizQuestion by the assigned ID
            var result = await repository.GetByIdAsync(quizQuestion.Id);

            // Assert that the retrieved QuizQuestion is not null and has the correct data
            Assert.NotNull(result);
            Assert.Equal("Sample Question", result.Question);

            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);

            // Add a QuizQuestion
            var quizQuestion = new WiseUpDude.Model.QuizQuestion
            {
                Question = "Old Question",
                QuestionType = WiseUpDude.Model.QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Old Explanation",
                QuizId = 1
            };

            await repository.AddAsync(quizQuestion); // Add the QuizQuestion

            // Retrieve the QuizQuestion by its ID to ensure it is saved
            var addedQuestion = await context.QuizQuestions.FirstOrDefaultAsync(q => q.Question == "Old Question");
            Assert.NotNull(addedQuestion);
            Assert.NotEqual(0, addedQuestion.Id); // Ensure the ID is set

            // Update the QuizQuestion
            quizQuestion.Id = addedQuestion.Id; // Set the correct ID
            quizQuestion.Question = "Updated Question";
            await repository.UpdateAsync(quizQuestion); // Update the QuizQuestion

            // Verify the update
            var result = await context.QuizQuestions.FindAsync(quizQuestion.Id);
            Assert.NotNull(result);
            Assert.Equal("Updated Question", result.Question);

            // Clean up the database
            await context.Database.EnsureDeletedAsync();
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveQuizQuestion()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizQuestionRepository(context);
            var quizQuestion = new QuizQuestion
            {
                Question = "To Be Deleted",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2" },
                Answer = "Option1",
                Explanation = "Explanation"
            };

            await repository.AddAsync(quizQuestion);
            await repository.DeleteAsync(quizQuestion.Id);

            var result = await context.QuizQuestions.FindAsync(quizQuestion.Id);

            Assert.Null(result);
        }
    }
}