using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WiseUpDude.Data;
using WiseUpDude.Model;
using WiseUpDude.Services;
using Xunit;

namespace WiseUpDude.Tests.Services
{
    public class QuizServiceTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly QuizService _quizService;

        public QuizServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _quizService = new QuizService(new HttpClient(), new ConfigurationBuilder().Build(), _dbContext);
        }

        [Fact]
        public async Task SaveQuizAsync_ShouldAddQuizToDatabase()
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
        public async Task GetQuizAsync_ShouldReturnQuizFromDatabase()
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
        public async Task UpdateQuizAsync_ShouldUpdateQuizInDatabase()
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
        public async Task DeleteQuizAsync_ShouldRemoveQuizFromDatabase()
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

        [Fact]
        public async Task AddQuizQuestionAsync_ShouldAddQuizQuestionToDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            var quizQuestion = new QuizQuestion
            {
                Question = "Sample Question",
                QuizId = quiz.Id
            };

            // Act
            await _quizService.AddQuizQuestionAsync(quizQuestion);

            // Assert
            var savedQuizQuestion = await _dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Question == "Sample Question");
            Assert.NotNull(savedQuizQuestion);
            Assert.Equal("Sample Question", savedQuizQuestion.Question);
            Assert.Equal(quiz.Id, savedQuizQuestion.QuizId);
        }

        [Fact]
        public async Task GetQuizQuestionAsync_ShouldReturnQuizQuestionFromDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            var quizQuestion = new QuizQuestion
            {
                Question = "Sample Question",
                QuizId = quiz.Id
            };
            await _dbContext.QuizQuestions.AddAsync(quizQuestion);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedQuizQuestion = await _quizService.GetQuizQuestionAsync(quizQuestion.Id);

            // Assert
            Assert.NotNull(retrievedQuizQuestion);
            Assert.Equal("Sample Question", retrievedQuizQuestion.Question);
            Assert.Equal(quiz.Id, retrievedQuizQuestion.QuizId);
        }

        [Fact]
        public async Task UpdateQuizQuestionAsync_ShouldUpdateQuizQuestionInDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            var quizQuestion = new QuizQuestion
            {
                Question = "Sample Question",
                QuizId = quiz.Id
            };
            await _dbContext.QuizQuestions.AddAsync(quizQuestion);
            await _dbContext.SaveChangesAsync();

            // Act
            quizQuestion.Question = "Updated Question";
            await _quizService.UpdateQuizQuestionAsync(quizQuestion);

            // Assert
            var updatedQuizQuestion = await _dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Id == quizQuestion.Id);
            Assert.NotNull(updatedQuizQuestion);
            Assert.Equal("Updated Question", updatedQuizQuestion.Question);
        }

        [Fact]
        public async Task DeleteQuizQuestionAsync_ShouldRemoveQuizQuestionFromDatabase()
        {
            // Arrange
            var quiz = new Quiz
            {
                Name = "Sample Quiz",
                UserId = "user1"
            };
            await _dbContext.Quizzes.AddAsync(quiz);
            await _dbContext.SaveChangesAsync();

            var quizQuestion = new QuizQuestion
            {
                Question = "Sample Question",
                QuizId = quiz.Id
            };
            await _dbContext.QuizQuestions.AddAsync(quizQuestion);
            await _dbContext.SaveChangesAsync();

            // Act
            await _quizService.DeleteQuizQuestionAsync(quizQuestion.Id);

            // Assert
            var deletedQuizQuestion = await _dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Id == quizQuestion.Id);
            Assert.Null(deletedQuizQuestion);
        }
    }
}
