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
            var repository = new QuizRepository(context);

            // Create a sample user
            var user = new ApplicationUser
            {
                Id = "1", // Assign a valid User Id
                UserName = "testuser"
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Create a sample quiz
            var quiz = new WiseUpDude.Model.Quiz
            {
                Name = "Sample Quiz",
                UserName = "testuser", // Assign a valid UserName
                Questions = new List<WiseUpDude.Model.QuizQuestion>
        {
            new WiseUpDude.Model.QuizQuestion
            {
                Question = "Sample Question",
                QuestionType = WiseUpDude.Model.QuizQuestionType.MultipleChoice,
                OptionsJson = "[\"Option1\", \"Option2\"]", // JSON string for OptionsJson
                Answer = "Option1",
                Explanation = "Sample Explanation"
            }
        }
            };

            // Add the Quiz
            await repository.AddAsync(quiz);

            // Retrieve the Quiz from the database to verify
            var result = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User for verification
                .FirstOrDefaultAsync();

            // Assertions
            Assert.NotNull(result); // Ensure the Quiz exists
            Assert.Equal("Sample Quiz", result.Name); // Verify the name
            Assert.Equal("testuser", result.User.UserName); // Verify the associated user
            Assert.NotNull(result.Questions.First().OptionsJson); // Verify the Options field is serialized correctly

            // Deserialize and verify the options
            var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(result.Questions.First().OptionsJson);
            Assert.Contains("Option1", options);
            Assert.Contains("Option2", options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllQuizzes()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository(context);

            // Create a sample user
            var user = new ApplicationUser
            {
                Id = "1", // Assign a valid User Id
                UserName = "testuser"
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var quizzes = new List<Quiz>
            {
                new Quiz { Name = "Quiz 1", UserName = "testuser", User = user  },
                new Quiz { Name = "Quiz 2", UserName = "testuser", User = user  }
            };

            foreach (var quiz in quizzes)
            {
                await repository.AddAsync(quiz);
            }

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository(context);

            // Create a sample user
            var user = new ApplicationUser
            {
                Id = "1", // Assign a valid User Id
                UserName = "testuser"
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var quiz = new Quiz { Name = "Sample Quiz", UserName = "testuser", User = user };

            await repository.AddAsync(quiz);
            var result = await repository.GetByIdAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Equal("Sample Quiz", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository(context);

            // Create a sample user
            var user = new ApplicationUser
            {
                Id = "1", // Assign a valid User Id
                UserName = "testuser"
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var quiz = new Quiz {Name = "Old Name", UserName = "testuser", User = user };

            await repository.AddAsync(quiz);
            quiz.Name = "Updated Name";
            await repository.UpdateAsync(quiz);

            var result = await context.Quizzes.FindAsync(quiz.Id);

            Assert.Equal("Updated Name", result.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveQuiz()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository(context);

            // Create a sample user
            var user = new ApplicationUser
            {
                Id = "1", // Assign a valid User Id
                UserName = "testuser"
            };

            // Add the user to the context
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var quiz = new Quiz {Name = "To Be Deleted", UserName = "testuser", User = user };

            await repository.AddAsync(quiz);
            await repository.DeleteAsync(quiz.Id);

            var result = await context.Quizzes.FindAsync(quiz.Id);

            Assert.Null(result);
        }
    }
}