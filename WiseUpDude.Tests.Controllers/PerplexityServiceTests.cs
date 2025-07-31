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
    }
}