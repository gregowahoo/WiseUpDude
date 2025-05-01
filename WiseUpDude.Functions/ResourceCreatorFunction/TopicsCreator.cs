using System;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Data.Repositories.Interfaces;
using WiseUpDude.Model;
using WiseUpDude.Services;

namespace ResourceCreatorFunction
{
    public class TopicsCreator
    {
        private readonly ILogger _logger;
        private readonly TopicService _topicService;
        private readonly TopicRepository _topicRepository;
        private readonly TopicCreationRunRepository _topicCreationRunRepository;
        private readonly string _llmName;
        //private readonly QuizQuestionRepository _quizQuestionRepository;

        public TopicsCreator(
            ILogger<TopicsCreator> logger,
            TopicService topicService,
            TopicRepository topicRepository,
            TopicCreationRunRepository topicCreationRunRepository,
            string llmName)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topicService = topicService;
            _topicRepository = topicRepository;
            _topicCreationRunRepository = topicCreationRunRepository;
            _llmName = llmName;
        }

        [Function("TopicsCreator")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo topicsCreatorTimer)
        {
            if (topicsCreatorTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {topicsCreatorTimer.ScheduleStatus.Next}");
            }

            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var uniqueTopics = (await _topicRepository.GetUniqueTopicsAsync()).ToList();

            if (uniqueTopics == null)
            {
                _logger.LogInformation("No unique topics found in the database.");
                return;
            }

            int maxIterations = 10; // Safeguard to prevent infinite loops
            int iteration = 0;

            while (uniqueTopics.Count <= 100 && iteration < maxIterations)
            {
                _logger.LogInformation($"Current unique topics count: {uniqueTopics.Count}. Fetching more topics...");

                var (topics, errorMessage) = await _topicService.GetRelevantQuizTopicsAsync(uniqueTopics);
                if (topics == null || errorMessage != null)
                {
                    _logger.LogError($"Failed to retrieve topics: {errorMessage}");
                    return;
                }

                var topicCreationRun = new WiseUpDude.Model.TopicCreationRun
                {
                    Llm = _llmName
                };

                try
                {
                    await _topicCreationRunRepository.AddAsync(topicCreationRun, topics);

                    _logger.LogInformation($"Successfully created TopicCreationRun with {topics.Count} topics.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to create TopicCreationRun: {ex.Message}");
                    return;
                }

                uniqueTopics = (await _topicRepository.GetUniqueTopicsAsync()).ToList();
                iteration++;
            }

            if (uniqueTopics.Count > 100)
            {
                _logger.LogInformation($"Successfully reached the target of more than 100 unique topics. Final count: {uniqueTopics.Count}");
            }
            else
            {
                _logger.LogWarning($"Loop terminated after {iteration} iterations without reaching the target. Current count: {uniqueTopics.Count}");
            }
        }
    }
}
