using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WiseUpDude.Data.Repositories;
using WiseUpDude.Model;
using WiseUpDude.Services;

namespace ResourceCreatorFunction
{
    public class QuizGenerator
    {
        private readonly ILogger _logger;
        private readonly TopicRepository _topicRepository;
        private readonly QuizRepository _quizRepository;
        private readonly QuizFromTopicService _quizQuestionsFromTopic;

        public QuizGenerator(
            ILogger<QuizGenerator> logger,
            TopicRepository topicRepository,
            QuizRepository quizRepository,
            QuizFromTopicService quizQuestionsFromTopic)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topicRepository = topicRepository;
            _quizRepository = quizRepository;
            _quizQuestionsFromTopic = quizQuestionsFromTopic;
        }

        [Function("QuizGenerator")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo quizGeneratorTimer)
        {
            // Log the next timer schedule
            if (quizGeneratorTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {quizGeneratorTimer.ScheduleStatus.Next}");
            }

            _logger.LogInformation($"QuizGenerator function executed at: {DateTime.Now}");

            // Fetch topics without questions
            _logger.LogInformation("Fetching topics without questions...");
            var topicsWithoutQuestions = await _topicRepository.GetTopicsWithoutQuestionsAsync();
            var topicCount = topicsWithoutQuestions.Count();
            _logger.LogInformation($"Found {topicCount} topic(s) without questions.");

            if (!topicsWithoutQuestions.Any())
            {
                _logger.LogInformation("No topics found without questions.");
                return;
            }

            foreach (var topic in topicsWithoutQuestions)
            {
                _logger.LogInformation($"Processing topic: Id={topic.Id}, Name={topic.Name}");

                try
                {
                    // Generate quiz for the topic
                    var criteria = new QuizRequestCriteria
                    {
                        Topic = topic.Name,
                        Difficulty = "Medium" // Set quiz-level difficulty
                    };

                    _logger.LogInformation($"Generating quiz for topic '{topic.Name}'...");
                    var quizResponse = await _quizQuestionsFromTopic.GenerateQuizAsync(criteria);

                    if (quizResponse != null)
                    {
                        _logger.LogInformation($"Quiz generated for topic '{topic.Name}'. Preparing to save...");

                        quizResponse.Type = "Topic";
                        quizResponse.Topic = topic.Name;
                        quizResponse.Difficulty = criteria.Difficulty; // Set quiz-level difficulty
                        quizResponse.Description = topic.Description;

                        foreach (var question in quizResponse.Questions)
                        {
                            question.Difficulty = criteria.Difficulty; // Set question-level difficulty
                        }

                        _logger.LogInformation($"Saving quiz for topic '{topic.Name}'...");
                        await _quizRepository.AddQuizAsync(quizResponse);
                        _logger.LogInformation($"Quiz successfully created and saved for topic: {topic.Name}");
                    }
                    else
                    {
                        _logger.LogWarning($"Quiz generation returned null for topic: {topic.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to generate quiz for topic {topic.Name}: {ex.Message}");
                }
            }
        }
    }
}
