namespace WiseUpDude.Services
{
    public static class QuizPromptTemplates
    {
        // This method is the old one, likely for Perplexity, and should not be used
        // for Gemini API calls where content is passed multimodally.
        // Keeping it as is, assuming it might be used elsewhere or for historical context.
        public static string BuildQuizPrompt(string urlOrPrompt)
        {
            var prompt = @"IMPORTANT: For multiple-choice questions, the correct answer must be randomly placed in one of the four options (A, B, C, or D), and the distribution of correct answer positions must be as even as possible across the quiz. For example, in a 20-question quiz, the correct answer should appear about 5 times in each position. Do NOT default to the first position. If you generate 4 questions, the correct answer should be in position 1 for one question, position 2 for one, position 3 for one, and position 4 for one. If the correct answer is not evenly distributed among the four positions, regenerate the quiz until this requirement is met.

For true/false questions: Always use exactly two answer options: [""True"", ""False""], in that order. The correct answer must be ""True"" for about half the questions and ""False"" for about half the questions. Do not default to ""True"" as the correct answer for most questions. If you generate 4 true/false questions, 2 should have ""True"" as the correct answer and 2 should have ""False"". If the correct answer is not evenly distributed between ""True"" and ""False"", regenerate the quiz until this requirement is met.

Create a quiz based on the following prompt: ""Use this URL or prompt: {0}""
The quiz should include as many questions as possible, preferably 20 question, up to a maximum of 25.
Include both multiple-choice and true/false questions.

QUESTION FORMATTING & ANSWER SHUFFLING:
For multiple-choice questions:
- Always create exactly 4 answer options.
- All answer options must be plausible and relevant to the question.
- Randomly assign the correct answer to either the 1st, 2nd, 3rd, or 4th position (A, B, C, or D). Do not default to the first position.
- In the entire quiz, balance the distribution of correct answer positions as evenly as possible, so the correct answer appears roughly 25% of the time in each position (i.e., if there are 20 questions, about 5 in each slot).
- Do NOT put the correct answer in the first position by default.
- For the multiple-choice questions, ensure that the distribution of correct answer positions is as even as possible. Do not allow any position to have more than a quarter of the total multiple-choice questions (rounded up).

For true/false questions:
- Always use exactly two answer options: [""True"", ""False""], in that order. Never shuffle or reverse these.
- The correct answer must be ""True"" for about half the questions and ""False"" for about half the questions. Do not default to ""True"" as the correct answer for most questions. If you generate 4 true/false questions, 2 should have ""True"" as the correct answer and 2 should have ""False"".

For all questions:
- Ensure the correct answers and explanations are factually accurate and grounded in widely accepted knowledge. If the prompt is about a specific domain, use official or well-regarded sources if applicable.
- Each question should be an object with: ""Question"", ""Options"", ""Answer"", ""Explanation"", ""QuestionType"", and ""Difficulty"".
- The ""QuestionType"" must be exactly ""TrueFalse"" or ""MultipleChoice"" (case-sensitive).
- The ""Difficulty"" must be one of: ""Easy"", ""Medium"", or ""Hard"". Distribute difficulties roughly evenly across the quiz.
- When including C# code in questions or explanations, format it so that each statement or line of code appears on its own line, using standard C# indentation and line breaks. Do not put multiple statements on a single line.

QUIZ DIFFICULTY:
- In addition to question-level difficulty, include a ""Difficulty"" property at the quiz (root) level. This should represent the overall difficulty of the quiz (e.g., based on the average or predominant difficulty of the questions). Set this to one of: ""Easy"", ""Medium"", or ""Hard"".

OUTPUT:
- Return only valid JSON in the following format:
{{ ""Questions"": [ {{ ""Question"": ""..."", ""Options"": [""...""], ""Answer"": ""..."", ""Explanation"": ""..."", ""QuestionType"": ""..."", ""Difficulty"": ""..."" }}, ... ], ""Type"": ""..."", ""Description"": ""..."", ""Difficulty"": ""..."" }}.
- Return only the raw JSON, without any code block formatting or prefixes like 'json'.
- Do NOT include any Markdown code block formatting (such as triple backticks or the word 'json') in your response. Return only the raw JSON.
- Do NOT include any explanation, commentary, or additional metadata (such as id, model, usage, citations, search_results, or any wrapper object). Return ONLY the quiz JSON object as specified above, and nothing else.

ERROR HANDLING:
- If the prompt is too vague, factually impossible, or cannot result in a meaningful quiz, return a JSON object in this format: {{ ""Error"": ""<reason>"" }}.
- If the prompt is ambiguous, choose the most likely intended topic based on the text. If still unclear, return the above error object explaining that the prompt was ambiguous.
";
            return string.Format(prompt, urlOrPrompt);
        }

        // This method is optimized for Gemini API calls where content is passed multimodally.
        // Updated to include explicit string examples for "QuestionType" in the OUTPUT format.
        public static string BuildQuizGenerationInstructions()
        {
            var prompt = @"
You are an expert quiz generator. Your task is to create a comprehensive and engaging quiz in a specific JSON format.

QUIZ CONTENT:
The quiz should include as many questions as possible, preferably 20 questions, up to a maximum of 25.
Include both multiple-choice and true/false questions.

QUESTION FORMATTING & ANSWER SHUFFLING:
For multiple-choice questions:
- Always create exactly 4 answer options.
- All answer options must be plausible and relevant to the question.
- Strive to randomly assign the correct answer to one of the four options (A, B, C, or D).
- In the entire quiz, aim to balance the distribution of correct answer positions as evenly as possible. For example, if there are 20 multiple-choice questions, the correct answer should appear roughly 5 times in each position. Do not default to the first position.

For true/false questions:
- Always use exactly two answer options: [""True"", ""False""], in that order. Never shuffle or reverse these.
- Aim for the correct answer to be ""True"" for about half the questions and ""False"" for about half the questions. Do not default to ""True"" as the correct answer for most questions.

For all questions:
- Ensure the correct answers and explanations are factually accurate and grounded in the provided context.
- Each question should be an object with the following properties: ""Question"", ""Options"", ""Answer"", ""Explanation"", ""QuestionType"", and ""Difficulty"".
- The ""QuestionType"" must be exactly ""TrueFalse"" or ""MultipleChoice"" (case-sensitive).
- The ""Difficulty"" must be one of: ""Easy"", ""Medium"", or ""Hard"". Distribute difficulties roughly evenly across the quiz.
- When including C# code in questions or explanations, format it so that each statement or line of code appears on its own line, using standard C# indentation and line breaks. Do not put multiple statements on a single line.

QUIZ DIFFICULTY:
- In addition to question-level difficulty, include a ""Difficulty"" property at the quiz (root) level. This should represent the overall difficulty of the quiz (e.g., based on the average or predominant difficulty of the questions). Set this to one of: ""Easy"", ""Medium"", or ""Hard"".

OUTPUT:
- Return only valid JSON in the following format:
{{
  ""Questions"": [
    {{
      ""Question"": ""..."",
      ""Options"": [""...""],
      ""Answer"": ""..."",
      ""Explanation"": ""..."",
      ""QuestionType"": ""MultipleChoice"", // Explicitly show string here
      ""Difficulty"": ""...""
    }},
    {{
      ""Question"": ""..."",
      ""Options"": [""True"", ""False""],
      ""Answer"": ""..."",
      ""Explanation"": ""..."",
      ""QuestionType"": ""TrueFalse"", // Explicitly show string here
      ""Difficulty"": ""...""
    }}
  ],
  ""Type"": ""..."",
  ""Description"": ""..."",
  ""Difficulty"": ""...""
}}.
- Return only the raw JSON, without any code block formatting or prefixes like 'json'.
- Do NOT include any Markdown code block formatting (such as triple backticks or the word 'json') in your response. Return only the raw JSON.
- Do NOT include any explanation, commentary, or additional metadata (such as id, model, usage, citations, search_results, or any wrapper object). Return ONLY the quiz JSON object as specified above, and nothing else.

ERROR HANDLING:
- If the provided content is too vague, factually impossible, or cannot result in a meaningful quiz, return a JSON object in this format: {{ ""Error"": ""<reason>"" }}.
- If the provided content is ambiguous, choose the most likely intended topic based on the text. If still unclear, return the above error object explaining that the content was ambiguous.
";
            return prompt.Trim(); // Trim any leading/trailing whitespace from the multiline string
        }
    }
}