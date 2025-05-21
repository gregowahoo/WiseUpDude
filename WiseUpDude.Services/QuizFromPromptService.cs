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
        var aiPrompt = BuildAIPrompt(prompt);
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

    private string BuildAIPrompt(string prompt)
    {
        return string.Join("\n", new[]
        {
            $"Create a quiz based on the following prompt: \"{prompt}\".",
            "The quiz should include at least 20 questions.",
            "Include both multiple-choice and true/false questions.",
            "",
            "QUESTION FORMATTING & ANSWER SHUFFLING:",
            "For multiple-choice questions:",
            "- Always create exactly 4 answer options.",
            "- All answer options must be plausible and relevant to the question.",
            "- Randomly assign the correct answer to either the 1st, 2nd, 3rd, or 4th position (A, B, C, or D). Do not default to the first position.",
            "- In the entire quiz, balance the distribution of correct answer positions as evenly as possible, so the correct answer appears roughly 25% of the time in each position (i.e., if there are 20 questions, about 5 in each slot).",
            "- Do NOT put the correct answer in the first position by default.",
            "- For the 20 multiple-choice questions, ensure that exactly 5 questions have the correct answer in position 1, 5 in position 2, 5 in position 3, and 5 in position 4. Track and enforce this distribution as you generate the quiz. Do not allow any position to have more than 5 correct answers.",
            "",
            "For true/false questions:",
            "- Always use exactly two answer options: [\"True\", \"False\"], in that order. Never shuffle or reverse these.",
            "- Ensure that, across all true/false questions, the correct answer is 'True' about half the time and 'False' about half the time.",
            "",
            "For all questions:",
            "- Ensure the correct answers and explanations are factually accurate and grounded in widely accepted knowledge. If the prompt is about a specific domain, use official or well-regarded sources if applicable.",
            "- Each question should be an object with: \"Question\", \"Options\", \"Answer\", \"Explanation\", and \"QuestionType\".",
            "- The \"QuestionType\" must be exactly \"TrueFalse\" or \"MultipleChoice\" (case-sensitive).",
            "",
            "OUTPUT:",
            "- Return only valid JSON in the following format:",
            "{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"Type\": \"...\", \"Description\": \"...\" }.",
            "- Return only the raw JSON, without any code block formatting or prefixes like 'json'.",
            "",
            "ERROR HANDLING:",
            "- If the prompt is too vague, factually impossible, or cannot result in a meaningful quiz, return a JSON object in this format: { \"Error\": \"<reason>\" }.",
            "- If the prompt is ambiguous, choose the most likely intended topic based on the text. If still unclear, return the above error object explaining that the prompt was ambiguous."
        });
    }
}