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
        //public async Task Run([TimerTrigger("%TopicGeneratorSchedule%")] TimerInfo myTimer)
        public async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo topicsCreatorTimer)
        {
            if (topicsCreatorTimer.ScheduleStatus is not null)                                              // Log the next timer schedule
            {
                _logger.LogInformation($"Next timer schedule at: {topicsCreatorTimer.ScheduleStatus.Next}");
            }

            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");               // Log the execution time

            var uniqueTopics = (await _topicRepository.GetUniqueTopicsAsync()).ToList();                    // Fetch unique topics from the repository

            if (uniqueTopics == null) // Check if uniqueTopics is null
            {
                _logger.LogInformation("No unique topics found in the database.");
                return;
            }

            if (uniqueTopics.Count > 100) // Check if there are more than 100 topics
            {
                _logger.LogInformation($"There are greater than 100 topics in the DB. Current count: {uniqueTopics.Count}");
                return;
            }

            var (topics, errorMessage) = await _topicService.GetRelevantQuizTopicsAsync(uniqueTopics);      // Retrieve topics using TopicService
            if (topics == null || errorMessage != null)
            {
                _logger.LogError($"Failed to retrieve topics: {errorMessage}");
                return;
            }

            var topicCreationRun = new WiseUpDude.Model.TopicCreationRun                    // Create a new TopicCreationRun record
            {
                Llm = _llmName
            };

            try
            {
                await _topicCreationRunRepository.AddAsync(topicCreationRun, topics);       // Persist the TopicCreationRun and associated topics to the database

                _logger.LogInformation($"Successfully created TopicCreationRun with {topics.Count} topics.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create TopicCreationRun: {ex.Message}");
            }
        }
    }
}
