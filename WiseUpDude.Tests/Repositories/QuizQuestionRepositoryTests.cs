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
    public class QuizQuestionRepositoryTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly QuizService _quizService;

        public QuizQuestionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _quizService = new QuizService(new HttpClient(), new ConfigurationBuilder().Build(), _dbContext);
        }

        [Fact]
        public async Task AddQuizQuestion_ShouldAddQuizQuestionToDatabase()
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
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2", "Option3" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
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
        public async Task GetQuizQuestion_ShouldReturnQuizQuestionFromDatabase()
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
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2", "Option3" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
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
        public async Task UpdateQuizQuestion_ShouldUpdateQuizQuestionInDatabase()
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
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2", "Option3" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
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
        public async Task DeleteQuizQuestion_ShouldRemoveQuizQuestionFromDatabase()
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
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string> { "Option1", "Option2", "Option3" },
                Answer = "Option1",
                Explanation = "Sample Explanation",
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
