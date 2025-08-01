using Microsoft.Extensions.Logging;

namespace WiseUpDude.Services
{
    public static class ContextualQuizPromptTemplates
    {
        /// <summary>
        /// Builds a prompt for quizzes generated from a user-provided topic or context.
        /// The API is allowed to use its own knowledge to generate questions.
        /// </summary>
        public static string BuildQuizPromptWithUserTopic(string userTopic, string? contextSummary = null, ILogger? logger = null)
        {
            var prompt = @$"
IMPORTANT: You are to create a quiz using your accurate and up-to-date general knowledge of the topic below.

TOPIC:
{userTopic}

{(string.IsNullOrWhiteSpace(contextSummary) ? "" : $"CONTEXT SUMMARY:\n{contextSummary}")}

CRITICAL REQUIREMENT – ANSWER/EXPLANATION CONSISTENCY:
- For every question, the most important task is to ensure that the 'Answer' field is logically and factually matched by the 'Explanation' field.
- Before finalizing each question, STOP and check:
    1. Does the explanation always support and justify why the answer is correct (for multiple-choice), or why the statement is true/false (for True/False)?
    2. If the explanation and 'Answer' do not match exactly—DO NOT KEEP the question. Regenerate the answer/explanation so they are in perfect agreement.
    3. For True/False questions, reread your explanation and then double-check: does it justify the answer without contradiction? The answer must NEVER contradict the explanation.

QUIZ INSTRUCTIONS:
- Generate 20–25 questions, both multiple-choice and true/false, using your general/world knowledge about the topic.
- For each question:
    - Determine the answer first, using only facts that you know for sure.
    - Write the question and plausible distractor options (for multiple choice).
    - Write an explanation that is clearly aligned with the answer.
    - Assign Difficulty: Easy, Medium, Hard.
    - Add a 'ContextSnippet' summarizing supporting info.
    - Add a 'Citation': a reputable book, journal, URL, or ['general knowledge'].
- For ALL questions, 'Answer' and 'Explanation' MUST match perfectly. If any mismatch is detected, fix or skip the question.

REQUIRED OUTPUT FORMAT:
{{
  ""Questions"": [
    {{
      ""Answer"": ""..."",
      ""Question"": ""..."",
      ""Options"": [""..."", ...],
      ""Explanation"": ""..."",
      ""QuestionType"": ""MultipleChoice"" | ""TrueFalse"",
      ""Difficulty"": ""Easy""|""Medium""|""Hard"",
      ""ContextSnippet"": ""..."",
      ""Citation"": [""...""] // e.g. URL, book, or ""general knowledge""
    }},
    ...
  ],
  ""Type"": ""..."",
  ""Description"": ""..."",
  ""Difficulty"": ""...""
}}

RULES:
- If the topic is too vague or not a real subject, return ONLY: {{ ""Error"": ""Insufficient topic detail for quiz generation."" }}
- Output only the raw JSON object. No markdown, explanations, or extra text.
";

            logger?.LogInformation("[QUIZ_PROMPT_TEMPLATE] BuildQuizPromptWithUserTopic generated prompt: {Prompt}", prompt);
            return prompt;
        }

        /// <summary>
        /// Builds a prompt for quizzes generated strictly from the content of a provided URL.
        /// The API must use ONLY the URL content (no outside knowledge).
        /// </summary>
        public static string BuildQuizPromptWithUrlContext(string sourceUrl, string? contextSummary = null, ILogger? logger = null)
        {
            var prompt = @$"
IMPORTANT: Create a quiz using ONLY the information found at the following URL—do NOT use any outside knowledge, guesses, or assumptions.

SOURCE URL TO USE:
{sourceUrl}

{(string.IsNullOrWhiteSpace(contextSummary) ? "" : $"CONTEXT SUMMARY:\n{contextSummary}")}

CRITICAL LOGIC REQUIREMENT – ANSWER/EXPLANATION CONSISTENCY:
- For every question, but especially True/False:
    1. Check and double-check: Does the explanation, word-for-word, fully support the given answer?
    2. If there is any disagreement, mismatch, or lack of justification between the 'Answer' and 'Explanation', you MUST regenerate the pair until they are perfectly matched or skip the question.
    3. For True/False, be extra rigorous. If the explanation does not justify the answer 100%, fix immediately.
    4. DO NOT use outside knowledge if the answer cannot be confirmed from the content—skip the question instead.

QUIZ INSTRUCTIONS:
- Generate 20–25 questions (multiple-choice and true/false), using ONLY the content at the provided URL.
- For each question:
    - The 'Answer' field must be directly justified by the 'Explanation', both grounded ONLY in the URL.
    - Write the question and plausible distractors (for multiple choice), or True/False as required.
    - Assign Difficulty: Easy, Medium, Hard.
    - Add a 'ContextSnippet' quoting or summarizing page content for support.
    - 'Citation' must always be [""{sourceUrl}""].

REQUIRED OUTPUT FORMAT:
{{
  ""Questions"": [
    {{
      ""Answer"": ""..."",
      ""Question"": ""..."",
      ""Options"": [""..."", ...],
      ""Explanation"": ""..."",
      ""QuestionType"": ""MultipleChoice"" | ""TrueFalse"",
      ""Difficulty"": ""Easy""|""Medium""|""Hard"",
      ""ContextSnippet"": ""..."",
      ""Citation"": [""{sourceUrl}""]
    }},
    ...
  ],
  ""Type"": ""..."",
  ""Description"": ""..."",
  ""Difficulty"": ""...""
}}

RULES:
- Never use facts or reasoning not verifiable from the provided URL content.
- Return only the raw JSON object. No markdown or extra text.
- If the content is insufficient, return ONLY: {{ ""Error"": ""URL lacks sufficient content for quiz generation."" }}
";

            logger?.LogInformation("[QUIZ_PROMPT_TEMPLATE] BuildQuizPromptWithUrlContext generated prompt: {Prompt}", prompt);
            return prompt;
        }
    }
}
