using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WiseUpDude.Data;
using WiseUpDude.Model;
using WiseUpDude.Services;
using Xunit;

namespace WiseUpDude.Tests.Repositories
{
    public class QuizRepositoryTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly QuizService _quizService;

        public QuizRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _quizService = new QuizService(new HttpClient(), new ConfigurationBuilder().Build(), _dbContext);
        }

        [Fact]
        public async Task AddQuiz_ShouldAddQuizToDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };

            // Act
            await _quizService.SaveQuizAsync(quiz);

            // Assert
            var savedQuiz = await _dbContext.Quizzes.FirstOrDefaultAsync(q => q.Name == "Sample Quiz");
            Assert.NotNull(savedQuiz);
            Assert.Equal("Sample Quiz", savedQuiz.Name);
            Assert.Equal("user1", savedQuiz.UserId);
        }

        [Fact]
        public async Task GetQuiz_ShouldReturnQuizFromDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedQuiz = await _quizService.GetQuizAsync(quiz.Id);

            // Assert
            Assert.NotNull(retrievedQuiz);
            Assert.Equal("Sample Quiz", retrievedQuiz.Name);
            Assert.Equal("user1", retrievedQuiz.UserId);
        }

        [Fact]
        public async Task UpdateQuiz_ShouldUpdateQuizInDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            // Act
            quiz.Name = "Updated Quiz";
            await _quizService.UpdateQuizAsync(quiz);

            // Assert
            var updatedQuiz = await _dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id == quiz.Id);
            Assert.NotNull(updatedQuiz);
            Assert.Equal("Updated Quiz", updatedQuiz.Name);
        }

        [Fact]
        public async Task DeleteQuiz_ShouldRemoveQuizFromDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            // Act
            await _quizService.DeleteQuizAsync(quiz.Id);

            // Assert
            var deletedQuiz = await _dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id == quiz.Id);
            Assert.Null(deletedQuiz);
        }
    }
}
