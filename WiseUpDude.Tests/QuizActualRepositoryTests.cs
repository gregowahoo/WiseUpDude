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
    public class QuizActualRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public QuizActualRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
                .Options;
        }

        private async Task<ApplicationDbContext> CreateDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=Beast;Database=WiseUpDude;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True") // Replace with your actual connection string
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync(); // Ensure the database is created
            return context;
        }

        [Fact]
        public async Task AddQuizWithQuestions_ShouldWriteToDatabase()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository(context);

            // Retrieve the user with the specified username from the database
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == "greg.ohlsen@gmail.com");
            Assert.NotNull(user); // Ensure the user exists

            // Create a sample quiz
            var quiz = new WiseUpDude.Model.Quiz
            {
                Name = "Sample Quiz",
                UserName = user.UserName,                           // Assign the user's username
                User = user,                                        // Assign the retrieved user
                Questions = new List<WiseUpDude.Model.QuizQuestion>
                {
                    new WiseUpDude.Model.QuizQuestion
                    {
                        Question = "What is 2 + 2?",
                        QuestionType = WiseUpDude.Model.QuizQuestionType.MultipleChoice,
                        OptionsJson = "[\"3\", \"4\", \"5\"]",      // JSON string for options
                        Answer = "4",
                        Explanation = "2 + 2 equals 4.",
                        UserAnswer = "4"                           // Add UserAnswer
                    },
                    new WiseUpDude.Model.QuizQuestion
                    {
                        Question = "What is the capital of France?",
                        QuestionType = WiseUpDude.Model.QuizQuestionType.MultipleChoice,
                        OptionsJson = "[\"Paris\", \"London\", \"Berlin\"]", // JSON string for options
                        Answer = "Paris",
                        Explanation = "The capital of France is Paris.",
                        UserAnswer = "Paris"                       // Add UserAnswer
                    }
                },
                QuizSource = new WiseUpDude.Model.QuizSource // Add QuizSource
                {
                    Type = "Topic",
                    Topic = "Math",
                    Description = "A quiz about basic math."
                }
            };

            // Add the quiz to the repository
            await repository.AddAsync(quiz);

            // Retrieve the quiz from the database
            var result = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User for verification
                .Include(q => q.QuizSource) // Include the QuizSource for verification
                .FirstOrDefaultAsync(q => q.Name == "Sample Quiz");

            // Assertions
            Assert.NotNull(result); // Ensure the quiz exists
            Assert.Equal("Sample Quiz", result.Name); // Verify the quiz name
            Assert.Equal("greg.ohlsen@gmail.com", result.User.UserName); // Verify the associated user
            Assert.Equal(2, result.Questions.Count); // Verify the number of questions

            // Verify the UserAnswer for each question
            Assert.Equal("4", result.Questions.First().UserAnswer); // Verify UserAnswer for the first question
            Assert.Equal("Paris", result.Questions.Last().UserAnswer); // Verify UserAnswer for the second question

            // Verify the QuizSource
            Assert.NotNull(result.QuizSource); // Ensure the QuizSource exists
            Assert.Equal("Topic", result.QuizSource.Type); // Verify the QuizSource type
            Assert.Equal("Math", result.QuizSource.Topic); // Verify the QuizSource topic
            Assert.Equal("A quiz about basic math.", result.QuizSource.Description); // Verify the QuizSource description
        }

        [Fact]
        public async Task DeleteQuizWithQuestions_ShouldRemoveOnlyOwnedQuizzes()
        {
            using var context = await CreateDbContextAsync();
            var repository = new QuizRepository(context);

            // Retrieve the user with the specified username from the database
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == "greg.ohlsen@gmail.com");
            Assert.NotNull(user); // Ensure the user exists

            // Retrieve the quiz added in AddQuizWithQuestions_ShouldWriteToDatabase
            var quizToDelete = await context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.User) // Include the User for verification
                .Include(q => q.QuizSource) // Include the QuizSource for verification
                .FirstOrDefaultAsync(q => q.Name == "Sample Quiz" && q.UserId == user.Id);

            Assert.NotNull(quizToDelete); // Ensure the quiz exists and is owned by the user

            // Store the quiz ID and QuizSource ID for later verification
            var quizId = quizToDelete.Id;
            var quizSourceId = quizToDelete.QuizSourceId;

            // Verify the UserAnswer for each question before deletion
            Assert.Equal("4", quizToDelete.Questions.First().UserAnswer); // Verify UserAnswer for the first question
            Assert.Equal("Paris", quizToDelete.Questions.Last().UserAnswer); // Verify UserAnswer for the second question

            // Delete the quiz
            await repository.DeleteAsync(quizToDelete.Id);

            // Verify the quiz is removed
            var deletedQuiz = await context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            Assert.Null(deletedQuiz); // Ensure the quiz no longer exists

            // Verify the quiz questions are also removed
            var remainingQuestions = await context.QuizQuestions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            Assert.Empty(remainingQuestions); // Ensure no questions remain for the deleted quiz

            // Verify the QuizSource is also removed
            var deletedQuizSource = await context.QuizSources.FirstOrDefaultAsync(qs => qs.Id == quizSourceId);
            Assert.Null(deletedQuizSource); // Ensure the QuizSource no longer exists

            // Verify the user still exists
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "greg.ohlsen@gmail.com");
            Assert.NotNull(existingUser); // Ensure the user still exists
            Assert.Equal(user.Id, existingUser.Id); // Verify the user ID matches
        }
    }
}
