using System.Text.Json;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class AnswerRandomizerService
    {
        public Quiz RandomizeAnswers(Quiz quiz)
        {
            return DistributeAnswersEvenly(quiz);
        }

        public Quiz DistributeAnswersEvenly(Quiz quiz)
        {
            var random = new Random();
            // Only process multiple choice questions for answer distribution
            var multipleChoiceQuestions = quiz.Questions
                .Where(q => q.QuestionType == QuizQuestionType.MultipleChoice && q.Options != null && q.Options.Count > 1)
                .ToList();

            if (multipleChoiceQuestions.Any())
            {
                ProcessMultipleChoiceQuestions(multipleChoiceQuestions, random);
            }
            // Do NOT process True/False questions for answer distribution
            return quiz;
        }

        private void ProcessMultipleChoiceQuestions(List<QuizQuestion> questions, Random random)
        {
            var positionCounts = new int[4]; // Track count for positions 0, 1, 2, 3
            var questionsToProcess = new List<(QuizQuestion question, string correctAnswer, int targetPosition)>();

            // First pass: determine target positions for even distribution
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var correctAnswer = question.Answer;
                
                if (string.IsNullOrEmpty(correctAnswer) || question.Options == null)
                    continue;

                // Find the position with the least assignments so far
                int targetPosition = 0;
                int minCount = positionCounts[0];
                for (int pos = 1; pos < Math.Min(4, question.Options.Count); pos++)
                {
                    if (positionCounts[pos] < minCount)
                    {
                        minCount = positionCounts[pos];
                        targetPosition = pos;
                    }
                }

                positionCounts[targetPosition]++;
                questionsToProcess.Add((question, correctAnswer, targetPosition));
            }

            // Second pass: arrange options to place correct answers in target positions
            foreach (var (question, correctAnswer, targetPosition) in questionsToProcess)
            {
                if (question.Options == null) continue;

                var options = new List<string>(question.Options);
                
                // Remove the correct answer from the list
                options.Remove(correctAnswer);
                
                // Shuffle the remaining incorrect options
                var shuffledIncorrect = options.OrderBy(_ => random.Next()).ToList();
                
                // Create new options list with correct answer in target position
                var newOptions = new List<string>();
                for (int i = 0; i < question.Options.Count; i++)
                {
                    if (i == targetPosition)
                    {
                        newOptions.Add(correctAnswer);
                    }
                    else
                    {
                        if (shuffledIncorrect.Any())
                        {
                            newOptions.Add(shuffledIncorrect[0]);
                            shuffledIncorrect.RemoveAt(0);
                        }
                    }
                }

                // Update the question with the new options arrangement
                question.Options = newOptions;
                question.Answer = correctAnswer; // Ensure correct answer is preserved
            }
        }
    }
}
