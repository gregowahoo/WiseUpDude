using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WiseUpDude.Controllers;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using Xunit;

namespace WiseUpDude.Tests.Controllers
{
    public class UserQuizAttemptsControllerTests
    {
        [Fact]
        public async Task GetById_ReturnsOk_WhenAttemptExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserQuizAttemptRepository<UserQuizAttempt>>();
            var mockLogger = new Mock<ILogger<UserQuizAttemptsController>>();
            var attempt = new UserQuizAttempt { Id = 1, UserQuizId = 2 };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(attempt);
            var controller = new UserQuizAttemptsController(mockRepo.Object, mockLogger.Object);

            // Act
            var result = await controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedAttempt = Assert.IsType<UserQuizAttempt>(okResult.Value);
            Assert.Equal(1, returnedAttempt.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenAttemptDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<IUserQuizAttemptRepository<UserQuizAttempt>>();
            var mockLogger = new Mock<ILogger<UserQuizAttemptsController>>();
            mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((UserQuizAttempt?)null);
            var controller = new UserQuizAttemptsController(mockRepo.Object, mockLogger.Object);

            // Act
            var result = await controller.GetById(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
