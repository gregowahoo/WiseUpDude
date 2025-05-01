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
            var topicsWithoutQuestions = await _topicRepository.GetTopicsWithoutQuestionsAsync();
            if (!topicsWithoutQuestions.Any())
            {
                _logger.LogInformation("No topics found without questions.");
                return;
            }

            foreach (var topic in topicsWithoutQuestions)
            {
                try
                {
                    // Generate quiz for the topic
                    var criteria = new QuizRequestCriteria
                    {
                        Topic = topic.Name,
                        Difficulty = "Medium" // Set quiz-level difficulty
                    };

                    var quizResponse = await _quizQuestionsFromTopic.GenerateQuizAsync(criteria);

                    if (quizResponse != null)
                    {
                        quizResponse.Type = "Topic";
                        quizResponse.Topic = topic.Name;
                        quizResponse.Difficulty = criteria.Difficulty; // Set quiz-level difficulty
                        quizResponse.Description = topic.Description;

                        foreach (var question in quizResponse.Questions)
                        {
                            question.Difficulty = criteria.Difficulty; // Set question-level difficulty
                        }

                        await _quizRepository.AddQuizAsync(quizResponse);
                        _logger.LogInformation($"Quiz successfully created for topic: {topic.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to generate quiz for topic {topic.Name}: {ex.Message}");
                }

                //// Pause for 1 minute to avoid hitting API rate limits
                //_logger.LogInformation("Pausing for 5 minutes before processing the next topic...");
                //await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }
    }
}
