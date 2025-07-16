namespace WiseUpDude.Services
{
    public static class ContextualQuizPromptTemplates
    {
        public static string BuildQuizPromptWithContext(string topicOrUrl, string? explicitContext)
        {
            var intro = @"
IMPORTANT: Base ALL questions, answers, and explanations ONLY on the content and explicit context provided from the following URL or prompt. Do NOT use outside knowledge, do NOT guess, and do NOT invent facts. If the content/context is insufficient, skip the question or return an error.

Return ONLY valid raw JSON do NOT include explanations, markdown, or any extra text.

Use the following content/context to generate a quiz:

{0}

{1}

Create a quiz using only the provided content/context. The quiz should include as many questions as possible, preferably 20 questions, up to a maximum of 25.
Include both multiple-choice and true/false questions.

QUESTION FORMATTING & ANSWER SHUFFLING:
For multiple-choice questions:
- Always create exactly 4 answer options.
- All answer options must be plausible and relevant to the question, and must be based on the provided content/context.
- Randomly assign the correct answer to either the 1st, 2nd, 3rd, or 4th position (A, B, C, or D). Do not default to the first position.
- In the ""Answer"" field, always return the full text of the correct answer option, not the letter (A, B, C, or D).
- In the entire quiz, balance the distribution of correct answer positions as evenly as possible, so the correct answer appears roughly 25% of the time in each position (i.e., if there are 20 questions, about 5 in each slot).
- Do NOT put the correct answer in the first position by default.
- For the multiple-choice questions, ensure that the distribution of correct answer positions is as even as possible. Do not allow any position to have more than a quarter of the total multiple-choice questions (rounded up).

For true/false questions:
- Always use exactly two answer options: [""True"", ""False""], in that order. Never shuffle or reverse these.
- The correct answer must be ""True"" for about half the questions and ""False"" for about half the questions. Do not default to ""True"" as the correct answer for most questions. If you generate 4 true/false questions, 2 should have ""True"" as the correct answer and 2 should have ""False"".
- If the correct answer is not evenly distributed between ""True"" and ""False"", regenerate the quiz until this requirement is met.

For all questions:
- Ensure the correct answers and explanations are factually accurate and grounded ONLY in the provided content/context. Do not use outside knowledge or assumptions.
- Each question should be an object with: ""Question"", ""Options"", ""Answer"", ""Explanation"", ""QuestionType"", ""Difficulty"", ""ContextSnippet"", and ""Citation"".
- The ""QuestionType"" must be exactly ""TrueFalse"" or ""MultipleChoice"" (case-sensitive).
- The ""Difficulty"" must be one of: ""Easy"", ""Medium"", or ""Hard"". Distribute difficulties roughly evenly across the quiz.
- When including C# code in questions or explanations, format it so that each statement or line of code appears on its own line, using standard C# indentation and line breaks. Do not put multiple statements on a single line.
- For each question:
  - Include a ""ContextSnippet"" field containing a brief 1-2 sentence supporting summary, quote, or excerpt from the content/context explaining why the question is relevant.
  - Include a ""Citation"" field (source URL, reference, or descriptor) for this context snippet if possible.

QUIZ DIFFICULTY:
- In addition to question-level difficulty, include a ""Difficulty"" property at the quiz (root) level. This should represent the overall difficulty of the quiz (e.g., based on the average or predominant difficulty of the questions). Set this to one of: ""Easy"", ""Medium"", or ""Hard"".

OUTPUT:
- Return only valid JSON in the following shape:
{{
    ""Questions"": [
        {{
            ""Question"": ""..."",
            ""Options"": [""..."", ""...""],
            ""Answer"": ""..."",
            ""Explanation"": ""..."",
            ""QuestionType"": ""MultipleChoice"", // or ""TrueFalse""
            ""Difficulty"": ""..."",
            ""ContextSnippet"": ""..."",
            ""Citation"": ""...""
        }},
        ...
    ],
    ""Type"": ""..."",
    ""Description"": ""..."",
    ""Difficulty"": ""...""
}}
- Return only the raw JSON, without any code block formatting or prefixes like 'json'.
- Do NOT include any Markdown code block formatting (such as triple backticks or the word 'json') in your response. Return only the raw JSON.
- Do NOT include any explanation, commentary, or additional metadata (such as id, model, usage, citations, search_results, or any wrapper object). Return ONLY the quiz JSON object as specified above, and nothing else.

ERROR HANDLING:
- If the provided content/context is too vague, factually impossible, or cannot result in a meaningful quiz, return a JSON object in this format: {{ ""Error"": ""<reason>"" }}.
- If the provided content/context is ambiguous, choose the most likely intended topic based on the text. If still unclear, return the above error object explaining that the content/context was ambiguous.
";

            return string.Format(intro, topicOrUrl, explicitContext == null ? "" : $"CONTEXT SUMMARY:\n{explicitContext}");
        }
    }
}
