using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiseUpDude.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonParser
    {
        public static Dictionary<string, object> ExtractAndInjectJson(Dictionary<string, object> completion)
        {
            try
            {
                // Retrieve the raw content from the first choice's message
                string rawContent = "";
                if (completion.TryGetValue("choices", out var choicesObj) && choicesObj is List<object> choicesList && choicesList.Count > 0)
                {
                    if (choicesList[0] is Dictionary<string, object> choiceDict &&
                        choiceDict.TryGetValue("message", out var messageObj) && messageObj is Dictionary<string, object> messageDict &&
                        messageDict.TryGetValue("content", out var contentObj) && contentObj is string contentStr)
                    {
                        rawContent = contentStr.Trim();
                    }
                }

                // Remove markdown code fences if present
                if (rawContent.StartsWith("```"))
                {
                    var lines = rawContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                    int start = 0, end = lines.Length;
                    if (lines[0].StartsWith("```")) start = 1;
                    if (lines.Length > 1 && lines[lines.Length - 1].StartsWith("```")) end = lines.Length - 1;
                    rawContent = string.Join("\n", lines, start, end - start).Trim();
                }

                // Try extracting JSON using the first and last brace positions
                int startBrace = rawContent.IndexOf('{');
                int endBrace = rawContent.LastIndexOf('}');
                string jsonCandidate = null;
                JObject parsedJson = null;

                if (startBrace != -1 && endBrace != -1 && startBrace < endBrace)
                {
                    jsonCandidate = rawContent.Substring(startBrace, endBrace - startBrace + 1).Trim();
                }

                // If substring extraction failed or candidate is unparseable, try regex fallback
                if (!string.IsNullOrEmpty(jsonCandidate))
                {
                    try
                    {
                        parsedJson = JObject.Parse(jsonCandidate);
                    }
                    catch (JsonReaderException)
                    {
                        var match = Regex.Match(rawContent, @"({.*})", RegexOptions.Singleline);
                        if (match.Success)
                        {
                            jsonCandidate = match.Groups[1].Value.Trim();
                            parsedJson = JObject.Parse(jsonCandidate);
                        }
                        else
                        {
                            throw new Exception("No valid JSON object found via regex.");
                        }
                    }
                }
                else
                {
                    var match = Regex.Match(rawContent, @"({.*})", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        jsonCandidate = match.Groups[1].Value.Trim();
                        parsedJson = JObject.Parse(jsonCandidate);
                    }
                    else
                    {
                        throw new Exception("No JSON object found in content.");
                    }
                }

                // Inject the parsed JSON back into the completion object
                var firstChoice = (Dictionary<string, object>)((List<object>)completion["choices"])[0];
                var message = (Dictionary<string, object>)firstChoice["message"];
                message["content"] = parsedJson;

                return completion;
            }
            catch (Exception e)
            {
                throw new Exception($"Error extracting valid JSON from content: {e.Message}");
            }
        }
    }
}
