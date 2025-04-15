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
                User = user, // Assign the user
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
                },
                QuizSource = new WiseUpDude.Model.QuizSource // Add QuizSource
                {
                    Type = "Topic",
                    Topic = "Math",
                    Description = "A quiz about basic math."
                }
            };

            // Add the Quiz
            await repository.AddAsync(quiz);

            // Retrieve the Quiz from the database to verify
            var result = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User for verification
                .Include(q => q.QuizSource) // Include the QuizSource for verification
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

            // Verify the QuizSource
            Assert.NotNull(result.QuizSource); // Ensure the QuizSource exists
            Assert.Equal("Topic", result.QuizSource.Type); // Verify the QuizSource type
            Assert.Equal("Math", result.QuizSource.Topic); // Verify the QuizSource topic
            Assert.Equal("A quiz about basic math.", result.QuizSource.Description); // Verify the QuizSource description
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
                new Quiz
                {
                    Name = "Quiz 1",
                    UserName = "testuser",
                    User = user,
                    QuizSource = new QuizSource
                    {
                        Type = "Topic",
                        Topic = "Science",
                        Description = "A quiz about science."
                    }
                },
                new Quiz
                {
                    Name = "Quiz 2",
                    UserName = "testuser",
                    User = user,
                    QuizSource = new QuizSource
                    {
                        Type = "Topic",
                        Topic = "History",
                        Description = "A quiz about history."
                    }
                }
            };

            foreach (var quiz in quizzes)
            {
                await repository.AddAsync(quiz);
            }

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains(result, q => q.Name == "Quiz 1" && q.QuizSource.Topic == "Science");
            Assert.Contains(result, q => q.Name == "Quiz 2" && q.QuizSource.Topic == "History");
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

            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserName = "testuser",
                User = user,
                QuizSource = new QuizSource
                {
                    Type = "Topic",
                    Topic = "Math",
                    Description = "A quiz about basic math."
                }
            };

            await repository.AddAsync(quiz);
            var result = await repository.GetByIdAsync(quiz.Id);

            Assert.NotNull(result);
            Assert.Equal("Sample Quiz", result.Name);
            Assert.Equal("Math", result.QuizSource.Topic);
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

            var quiz = new Quiz
            {
                Name = "Old Name",
                UserName = "testuser",
                User = user,
                QuizSource = new QuizSource
                {
                    Type = "Topic",
                    Topic = "Math",
                    Description = "A quiz about basic math."
                }
            };

            await repository.AddAsync(quiz);

            // Update the quiz
            quiz.Name = "Updated Name";
            quiz.QuizSource.Topic = "Updated Topic";
            await repository.UpdateAsync(quiz);

            var result = await context.Quizzes
                .Include(q => q.QuizSource)
                .FirstOrDefaultAsync(q => q.Id == quiz.Id);

            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Topic", result.QuizSource.Topic);
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

            var quiz = new Quiz
            {
                Name = "To Be Deleted",
                UserName = "testuser",
                User = user,
                QuizSource = new QuizSource
                {
                    Type = "Topic",
                    Topic = "Math",
                    Description = "A quiz about basic math."
                }
            };

            await repository.AddAsync(quiz);
            await repository.DeleteAsync(quiz.Id);

            var result = await context.Quizzes.FindAsync(quiz.Id);
            var quizSource = await context.QuizSources.FindAsync(quiz.QuizSource.Id);

            Assert.Null(result);
            Assert.Null(quizSource);
        }
    }
}