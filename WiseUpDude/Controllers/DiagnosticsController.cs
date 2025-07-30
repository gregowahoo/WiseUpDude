using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;

namespace WiseUpDude.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IConfiguration _configuration;

        public DiagnosticsController(TelemetryClient telemetryClient, IConfiguration configuration)
        {
            _telemetryClient = telemetryClient;
            _configuration = configuration;
        }

        [HttpGet("test-app-insights")]
        public IActionResult TestAppInsights()
        {
            // Get information about the Application Insights configuration
            var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            var configConnectionString = _configuration["ApplicationInsights:ConnectionString"];
            var instrumentationKey = _configuration["ApplicationInsights:InstrumentationKey"];
            
            // Use reflection to get the actual channel being used
            var channelInfo = "Unknown";
            var channel = typeof(TelemetryClient)
                .GetField("_channel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                .GetValue(_telemetryClient) as ITelemetryChannel;
            
            if (channel != null)
            {
                channelInfo = channel.GetType().FullName ?? "Unknown";
            }

            // Send a test event to Application Insights
            var properties = new Dictionary<string, string>
            {
                ["TestSource"] = "DiagnosticsController",
                ["TestTime"] = DateTime.UtcNow.ToString("o")
            };
            
            _telemetryClient.TrackEvent("TestAppInsightsFromDiagnosticsController", properties);
            _telemetryClient.Flush();
            
            // Log using Serilog too
            Serilog.Log.Information("?? Diagnostics test from controller. {@Properties}", 
                new { TestSource = "DiagnosticsController", TestTime = DateTime.UtcNow });

            // Create the diagnostic result
            var result = new
            {
                ConnectionStringFromEnv = connectionString != null 
                    ? $"Present (length: {connectionString.Length})" 
                    : "Not found",
                ConnectionStringFromConfig = configConnectionString != null 
                    ? $"Present (length: {configConnectionString.Length})" 
                    : "Not found",
                InstrumentationKeyFromConfig = instrumentationKey != null 
                    ? $"Present (length: {instrumentationKey.Length})" 
                    : "Not found",
                TelemetryClientInitialized = _telemetryClient != null,
                TelemetryChannel = channelInfo,
                EventSent = "TestAppInsightsFromDiagnosticsController",
                Message = "Test event sent to Application Insights. Check logs in a few minutes."
            };

            return Ok(result);
        }
    }
}