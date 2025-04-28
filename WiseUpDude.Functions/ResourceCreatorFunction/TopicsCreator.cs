using System;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Entities;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;
using WiseUpDude.Services;

namespace ResourceCreatorFunction
{
    public class TopicsCreator
    {
        private readonly ILogger _logger;
        private readonly QuizTopicService _quizTopicService;
        private readonly TopicRepository _topicRepository;
        private readonly TopicCreationRunRepository _topicCreationRunRepository;
        private readonly string _llmName;
        //private readonly QuizQuestionRepository _quizQuestionRepository;

        public TopicsCreator(
            ILogger<TopicsCreator> logger,
            QuizTopicService quizTopicService,
            TopicRepository topicRepository,
            TopicCreationRunRepository topicCreationRunRepository,
            string llmName)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _quizTopicService = quizTopicService;
            _topicRepository = topicRepository;
            _topicCreationRunRepository = topicCreationRunRepository;
            _llmName = llmName;
        }

        [Function("TopicsCreator")]
        public async Task Run([TimerTrigger("%TopicGeneratorSchedule%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            // Retrieve topics using QuizTopicService
            var (topics, errorMessage) = await _quizTopicService.GetRelevantQuizTopicsAsync();
            if (topics == null || errorMessage != null)
            {
                _logger.LogError($"Failed to retrieve topics: {errorMessage}");
                return;
            }

            // Create a new TopicCreationRun record
            var topicCreationRun = new WiseUpDude.Model.TopicCreationRun
            {
                Llm = _llmName
            };

            try
            {
                // Persist the TopicCreationRun and associated topics to the database
                await _topicCreationRunRepository.AddAsync(topicCreationRun, topics);
                _logger.LogInformation($"Successfully created TopicCreationRun with {topics.Count} topics.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create TopicCreationRun: {ex.Message}");
            }
        }
    }
}
