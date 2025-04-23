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
        private readonly QuizQuestionsFromTopic _quizQuestionsFromTopic;

        public QuizGenerator(
            ILoggerFactory loggerFactory,
            TopicRepository topicRepository,
            QuizRepository quizRepository,
            QuizQuestionsFromTopic quizQuestionsFromTopic)
        {
            _logger = loggerFactory.CreateLogger<QuizGenerator>();
            _topicRepository = topicRepository;
            _quizRepository = quizRepository;
            _quizQuestionsFromTopic = quizQuestionsFromTopic;
        }

        [Function("QuizGenerator")]
        public async Task Run([TimerTrigger("%QuizGeneratorSchedule%")] TimerInfo myTimer) // Use a settings variable for the schedule
        {
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
                        Difficulty = "Medium"
                    };
                    var quizResponse = await _quizQuestionsFromTopic.GenerateQuizAsync(criteria);

                    if (quizResponse != null)
                    {
                        // Ensure the topic is set in the QuizSource
                        quizResponse.QuizSource.Topic = topic.Name;

                        // Persist the quiz, questions, and quiz source
                        await _quizRepository.AddQuizAsync(quizResponse);
                        _logger.LogInformation($"Quiz successfully created for topic: {topic.Name}");
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
