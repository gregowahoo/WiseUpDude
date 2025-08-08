using Moq;
using WiseUpDude.Model;
using WiseUpDude.Shared.Services.Interfaces;

namespace WiseUpDude.Test.Shared;

public class UnitTest1
{
    [Fact]
    public async Task CreateAsync_ReturnsCreatedAttempt()
    {
        // Arrange
        var mockService = new Mock<IUserQuizAttemptApiService>();
        var attempt = new UserQuizAttempt { Id = 1, UserQuizId = 2 };
        mockService.Setup(s => s.CreateAsync(It.IsAny<UserQuizAttempt>()))
                   .ReturnsAsync(attempt);

        // Act
        var result = await mockService.Object.CreateAsync(new UserQuizAttempt());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(2, result.UserQuizId);
    }
}
