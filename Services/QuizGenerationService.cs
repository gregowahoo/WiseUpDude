using System.Collections.Generic;
using WiseUpDude.Model;

namespace WiseUpDude.Services
{
    public class QuizGenerationService
    {
        // This method generates sample questions.
        public List<QuizQuestion> GenerateQuiz(string content)
        {
            var questions = new List<QuizQuestion>();

            // Example True/False question
            questions.Add(new QuizQuestion
            {
                Question = "Is the extracted content non-empty?",
                QuestionType = QuizQuestionType.TrueFalse,
                Answer = "True",
                Explanation = "Since the content was successfully fetched, it should not be empty.",
                Options = new List<string> { "True", "False" }
            });

            // Example Multiple Choice question
            questions.Add(new QuizQuestion
            {
                Question = "What best describes the content retrieved from the URL?",
                QuestionType = QuizQuestionType.MultipleChoice,
                Options = new List<string>
                {
                    "Mostly textual information",
                    "Primarily images",
                    "Mostly videos",
                    "A mix of text and images"
                },
                Answer = "Mostly textual information",
                Explanation = "Since the service extracts the text from the body tag, it implies that the content is primarily textual."
            });

            return questions;
        }
    }
}
