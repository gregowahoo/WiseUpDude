using System.Text.Json;
using WiseUpDude.Model;
using WiseUpDude.Data.Repositories;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Identity.Client;
using System.Linq;
using System.Text;

namespace WiseUpDude.Services
{
    public class ContentCreatorService
    {
        private readonly IChatClient _chatClient;

        public ContentCreatorService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<(List<Topic>? Topics, List<QuizResponse>? Quizzes)> GenerateContentAsync()
        {
            // Fetch existing topics from a repository or API
            var existingTopics = await GetExistingTopicsAsync();

            // Combine prompt to fetch new topics and generate quizzes
            var prompt = GeneratePrompt(existingTopics);

            var result = await _chatClient.GetResponseAsync(prompt);
            var content = result.Text;

            // Parse the response into topics and quizzes
            return ParseResponse(content);
        }

        public async Task SaveContentAsync(List<Topic> topics, List<QuizResponse> quizzes)
        {
            // Placeholder for saving topics and quizzes to a database or repository
            await SaveTopicsAndQuizzesAsync(topics, quizzes);
        }

        private async Task<List<string>> GetExistingTopicsAsync()
        {
            // Placeholder for fetching existing topics from a database or API
            return new List<string> { "Science", "History" }; // Example existing topics
        }

        private string GeneratePrompt(List<string> existingTopics)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are a helpful assistant that creates engaging quizzes.");
            sb.AppendLine("Generate a list of 20 interesting and current quiz topics (excluding Science and History). Each topic must include a 'name' and a 1-sentence 'description' explaining why it's interesting or relevant.");
            sb.AppendLine("For each topic, create a quiz containing at least 5 questions. Use a mix of multiple-choice and true/false questions (for true/false, the options must always be [\"True\", \"False\"]).");
            sb.AppendLine("Each question should include: \"Question\" (the quiz question), \"Options\" (an array of possible answers), \"Answer\" (the correct answer), \"Explanation\" (a brief explanation of the answer), and \"QuestionType\" (either \"MultipleChoice\" or \"TrueFalse\").");
            sb.AppendLine("Use three difficulty levels: Easy (basic knowledge), Medium (moderate understanding), and Hard (advanced understanding). Adjust question complexity and vocabulary based on this scale.");
            sb.AppendLine("Return a single valid JSON object with two keys: \"Topics\" (an array of topic objects) and \"Quizzes\" (an array of quiz objects, each with the following structure: { \"Questions\": [ ... ], \"Type\": \"quiz\", \"Topic\": \"...\", \"Description\": \"...\" }).");
            sb.AppendLine("Do not include Science or History topics.");
            sb.AppendLine("Return only the raw JSON, with no introduction, closing, or code block formatting.");
            sb.AppendLine("Example Output Structure: { \"Topics\": [ {\"name\": \"Pop Culture Trends\", \"description\": \"Test your knowledge of the latest movies, music, and viral moments.\"}, ... ], \"Quizzes\": [ { \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"Type\": \"quiz\", \"Topic\": \"...\", \"Description\": \"...\" }, ... ] }");

            return sb.ToString();
                
            //var stringBuilder = new StringBuilder();

            //stringBuilder.AppendLine("You are a helpful assistant that suggests topics for short, engaging quizzes.");
            //stringBuilder.AppendLine("Can you create a list of at least 20 topics that would be interesting for people to take a quiz on?");
            //stringBuilder.AppendLine("Only topics that will be able to be used to create at least 20 questions from.");
            //stringBuilder.AppendLine("Topics should be interesting and current. Each topic should include a short 1-sentence description");
            //stringBuilder.AppendLine("explaining why it's interesting or relevant.");
            //stringBuilder.AppendLine();
            //stringBuilder.AppendLine("Return a JSON array of 40 objects, each with 'name' and 'description'.");
            //stringBuilder.AppendLine("Format for Topics: [{\"name\":\"...\",\"description\":\"...\"}, ...]. No intro or closing.");
            //stringBuilder.AppendLine("Return only the raw JSON without any code block formatting or prefixes like 'json'.");
            //stringBuilder.AppendLine($"Exclude the following topics: {string.Join(", ", existingTopics)}.");
            //stringBuilder.AppendLine();
            //stringBuilder.AppendLine("For each topic you created, try to create:");
            //stringBuilder.AppendLine("- A quizResponse for each topic.");
            //stringBuilder.AppendLine("- Use the following difficulty scale: Easy (basic knowledge), Medium (moderate understanding), Hard (advanced understanding).");
            //stringBuilder.AppendLine("- Adjust the question complexity and vocabulary based on this scale.");
            //stringBuilder.AppendLine("- Try to create at least 20 questions for each topic you created.");
            //stringBuilder.AppendLine("- Include both multiple-choice and true/false questions.");
            //stringBuilder.AppendLine("- For true/false questions, the options must always be: [\"True\", \"False\"].");
            //stringBuilder.AppendLine();
            //stringBuilder.AppendLine("Each question should be an object with:");
            //stringBuilder.AppendLine("\"Question\", \"Options\", \"Answer\", \"Explanation\", \"QuestionType\".");
            //stringBuilder.AppendLine("The \"QuestionType\" should be either \"TrueFalse\" or \"MultipleChoice\" depending on the type of question.");
            //stringBuilder.AppendLine();
            //stringBuilder.AppendLine("Return only valid JSON in the format:");
            //stringBuilder.AppendLine("{ \"Questions\": [ { \"Question\": \"...\", \"Options\": [\"...\"], \"Answer\": \"...\", \"Explanation\": \"...\", \"QuestionType\": \"...\" }, ... ], \"Type\": \"...\", \"Topic\": \"...\", \"Description\": \"...\" }.");
            //stringBuilder.AppendLine("Return only the raw JSON without any code block formatting or prefixes like 'json'.");

            //return stringBuilder.ToString();
        }


        //private string GeneratePrompt(List<string> existingTopics)
        //{
        //    return string.Join("\n", new[]
        //    {
        //        "You are a helpful assistant that suggests topics for short, engaging quizzes. " +
        //        "Can you create a list of at least 20 topics that would be interesting for people to take a quiz on? " +
        //        "Only topics that will be able to be used to create at least 20 questions from. " +
        //        "Topics should be interesting and current. Each topic should include a short 1-sentence description " +
        //        "explaining why it's interesting or relevant. \n\n" +

        //        "Return a JSON array of 40 objects, each with 'name' and 'description'. " +
        //        "Format: [{\"name\":\"...\",\"description\":\"...\"}, ...]. No intro or closing. " +
        //        "Return only the raw JSON without any code block formatting or prefixes like 'json'." +
        //        $"Exclude the following topics: {string.Join(", ", existingTopics)}."

        //    //"You are a helpful assistant that suggests topics and generates quizzes.",
        //    //    "Create a JSON object with two properties: 'Topics' and 'Quizzes'.",
        //    //    "'Topics' should be an array of 20 objects, each with 'name' and 'description'.",
        //    //    $"Exclude the following topics: {string.Join(", ", existingTopics)}.",
        //    //    "'Quizzes' should be an array of quiz objects, each corresponding to a topic in 'Topics'.",
        //    //    "Each quiz object should have: 'Questions', 'Type', 'Topic', 'Description'.",
        //    //    "Each question should have: 'Question', 'Options', 'Answer', 'Explanation', 'QuestionType'.",
        //    //    "Ensure that each topic has at least 20 questions in its corresponding quiz.",
        //    //    "Topics should be interesting, current, and able to generate at least 20 questions.",
        //    //    "Return only valid JSON without any code block formatting or prefixes like 'json'."
        //    });
        //}

        private (List<Topic>? Topics, List<QuizResponse>? Quizzes) ParseResponse(string content)
        {
            try
            {
                var json = JsonDocument.Parse(content);
                var topics = JsonSerializer.Deserialize<List<Topic>>(json.RootElement.GetProperty("Topics").ToString());
                var quizzes = JsonSerializer.Deserialize<List<QuizResponse>>(json.RootElement.GetProperty("Quizzes").ToString());
                return (topics, quizzes);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                return (null, null);
            }
        }

        private async Task SaveTopicsAndQuizzesAsync(List<Topic> topics, List<QuizResponse> quizzes)
        {
            // Placeholder for saving topics and quizzes to a database or repository
            await Task.CompletedTask;
        }
    }
}
