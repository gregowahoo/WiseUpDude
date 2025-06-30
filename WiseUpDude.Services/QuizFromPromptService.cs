using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WiseUpDude.Model;
using WiseUpDude.Services.Interfaces;
using WiseUpDude.Services;

public class QuizFromPromptService : IQuizFromPromptService
{
    private readonly IChatClient _chatClient;
    private readonly AnswerRandomizerService _answerRandomizer;
    private readonly ILogger<QuizFromPromptService> _logger;

    public QuizFromPromptService(
        IChatClient chatClient,
        AnswerRandomizerService answerRandomizer,
        ILogger<QuizFromPromptService> logger)
    {
        _chatClient = chatClient;
        _answerRandomizer = answerRandomizer;
        _logger = logger;
    }

    public async Task<List<QuizQuestion>?> GenerateQuestionsFromPromptAsync(string prompt)
    {
        var aiPrompt = QuizPromptTemplates.BuildQuizPrompt(prompt);
        _logger.LogInformation("Sending AI prompt: {AiPrompt}", aiPrompt);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new QuizQuestionTypeConverter() }
        };

        var result = await _chatClient.GetResponseAsync(aiPrompt);
        var json = result.Text;

        _logger.LogInformation("Raw AI API Response: {Json}", json);

        if (!string.IsNullOrWhiteSpace(json) && json.TrimStart().StartsWith("{\"Error\"", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("AI API returned an error: {Json}", json);
            return null;
        }

        var questions = await TryParseAndRandomizeQuestionsAsync(json, options);
        if (questions is null || !questions.Any())
        {
            _logger.LogError("Quiz question generation failed.");
            return null;
        }

        return questions;
    }

    private Task<List<QuizQuestion>?> TryParseAndRandomizeQuestionsAsync(string json, JsonSerializerOptions options)
    {
        try
        {
            var quiz = JsonSerializer.Deserialize<Quiz>(json, options);
            if (quiz?.Questions != null && quiz.Questions.Any())
            {
                var randomizedQuiz = _answerRandomizer.RandomizeAnswers(quiz);

                return Task.FromResult<List<QuizQuestion>?>(randomizedQuiz.Questions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse or randomize questions.");
        }
        return Task.FromResult<List<QuizQuestion>?>(null);
    }
}