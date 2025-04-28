using System;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WiseUpDude.Services;

namespace ResourceCreatorFunction
{
    public class ContentCreatorFunction
    {
        private readonly ILogger _logger;
        private readonly ContentCreatorService _contentCreatorService;

        public ContentCreatorFunction(ILogger<QuizGenerator> logger, ContentCreatorService contentCreatorService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contentCreatorService = contentCreatorService;
        }

        [Function("ContentCreatorFunction")]
        public async Task Run([TimerTrigger("* * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
            else
            {
                return; // Exit if the schedule status is null
            }

            try
            {
                var (topics, quizzes) = await _contentCreatorService.GenerateContentAsync();

                if (topics == null || quizzes == null)
                {
                    _logger.LogError("Failed to parse topics or quizzes from the response.");
                    return;
                }

                await _contentCreatorService.SaveContentAsync(topics, quizzes);

                _logger.LogInformation("Successfully processed topics and quizzes.");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"JSON parsing error in ContentCreatorFunction: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in ContentCreatorFunction: {ex.Message}");
            }

        }
    }
}
