using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WiseUpDude.Tests.Controllers;

public class ApplicationInsightsConfigurationTests
{
    [Fact]
    public void ApplicationInsights_Configuration_ShouldBeReadable()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Act
        var connectionString = configuration["ApplicationInsights:ConnectionString"];
        var instrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];

        // Assert
        // The configuration section should exist (even if empty)
        Assert.NotNull(connectionString);
        
        // Should be able to read both ConnectionString and InstrumentationKey paths
        // (InstrumentationKey may be null if not configured, which is fine)
        Assert.True(connectionString != null || instrumentationKey != null);
    }

    [Fact]
    public void ApplicationInsights_ConnectionString_ShouldHaveCorrectConfigPath()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Act
        var aiSection = configuration.GetSection("ApplicationInsights");
        var connectionString = aiSection["ConnectionString"];

        // Assert
        // Verify that the ApplicationInsights section exists and has the expected structure
        Assert.True(aiSection.Exists(), "ApplicationInsights configuration section should exist");
        Assert.NotNull(connectionString);
        
        // Connection string can be empty (will be set via environment variables in Azure)
        // but the configuration path should be accessible
        Assert.True(connectionString != null, "ConnectionString configuration key should be accessible");
    }
}