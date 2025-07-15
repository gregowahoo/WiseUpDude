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

Create a quiz using only the provided content/context. The quiz should include as many questions as possible, up to 25.

For each question:
- Include a ""ContextSnippet"" field containing a brief 1-2 sentence supporting summary, quote, or excerpt from the content/context explaining why the question is relevant.
- Include a ""Citation"" field (source URL, reference, or descriptor) for this context snippet if possible.

QUESTION FORMATTING:
- Multiple choice: always 4 plausible options, correct answer randomly assigned, options shuffled.
- True/false: 2 options [""True"", ""False""], with correct answers evenly split.
- Each question object: ""Question"", ""Options"", ""Answer"", ""Explanation"", ""QuestionType"", ""Difficulty"", ""ContextSnippet"", ""Citation"".

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
";

            return string.Format(intro, topicOrUrl, explicitContext == null ? "" : $"CONTEXT SUMMARY:\n{explicitContext}");
        }
    }
}
