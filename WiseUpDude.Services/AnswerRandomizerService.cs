using System.Text.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class AnswerRandomizerService
    {
        public QuizResponse RandomizeAnswers(QuizResponse quizResponse)
        {
            var random = new Random();

            foreach (var question in quizResponse.Questions)
            {
                if (question.Options != null && question.Options.Any())
                {
                    // Store the correct answer before shuffling
                    var correctAnswer = question.Answer;

                    // Shuffle the options
                    var shuffledOptions = question.Options.OrderBy(_ => random.Next()).ToList();

                    // Update the options
                    question.Options = shuffledOptions;

                    // Reassign the correct answer based on its new position in the shuffled list
                    question.Answer = shuffledOptions.Contains(correctAnswer) ? correctAnswer : null;

                    // Log a warning if the correct answer is missing after shuffling
                    if (question.Answer == null)
                    {
                        Console.WriteLine($"Warning: Correct answer '{correctAnswer}' not found in shuffled options for question: {question.Question}");
                    }
                }
            }

            return quizResponse;
        }
    }
}
