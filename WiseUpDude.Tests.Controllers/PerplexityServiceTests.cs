using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using WiseUpDude.Services;
using Xunit;

namespace WiseUpDude.Tests.Controllers
{
    public class PerplexityServiceTests
    {
        [Fact]
        public void PerplexityService_Constructor_AcceptsAnswerRandomizerService()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<PerplexityService>>();
            var mockUrlMetaLogger = new Mock<ILogger<UrlMetaService>>();
            var answerRandomizer = new AnswerRandomizerService();

            // Create real instances with minimal dependencies
            var httpClient = new HttpClient();
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            var contentFetchingService = new ContentFetchingService(httpClient);
            var urlMetaService = new UrlMetaService(httpClient, mockUrlMetaLogger.Object);

            // Act & Assert - This should not throw an exception
            var service = new PerplexityService(
                mockHttpClientFactory.Object,
                contentFetchingService,
                mockLogger.Object,
                urlMetaService,
                answerRandomizer);

            Assert.NotNull(service);
        }

        [Fact]
        public void QuizPromptTemplates_BuildQuizPrompt_ContainsVerificationInstructions()
        {
            // Arrange
            string testUrl = "https://example.com/test-content";

            // Act
            string prompt = QuizPromptTemplates.BuildQuizPrompt(testUrl);

            // Assert - Verify that our new verification instructions are present
            Assert.Contains("VERIFICATION REQUIREMENT", prompt);
            Assert.Contains("double-check the answer against the provided content", prompt);
            Assert.Contains("FINAL VERIFICATION", prompt);
            Assert.Contains("review each question and answer one more time", prompt);
            Assert.Contains("Every answer is directly supported by the provided content", prompt);
            Assert.Contains("No external knowledge was inadvertently included", prompt);
        }
    }
}