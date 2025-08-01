using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WiseUpDude.Services
{
    public class JsonExtractor
    {
        public static JObject ExtractValidJson(Dictionary<string, object> response)
        {
            // Navigate to content field
            string content = "";
            if (response.TryGetValue("choices", out var choicesObj) && choicesObj is List<object> choicesList && choicesList.Count > 0)
            {
                if (choicesList[0] is Dictionary<string, object> choiceDict)
                {
                    if (choiceDict.TryGetValue("message", out var messageObj) && messageObj is Dictionary<string, object> messageDict)
                    {
                        if (messageDict.TryGetValue("content", out var contentObj) && contentObj is string contentStr)
                        {
                            content = contentStr;
                        }
                    }
                }
            }

            const string marker = "</think>";
            int idx = content.LastIndexOf(marker, StringComparison.Ordinal);

            string jsonStr;
            if (idx == -1)
            {
                jsonStr = content.Trim();
            }
            else
            {
                jsonStr = content.Substring(idx + marker.Length).Trim();
            }

            // Remove markdown code fence markers
            if (jsonStr.StartsWith("```json"))
                jsonStr = jsonStr.Substring(7).Trim();
            if (jsonStr.StartsWith("```"))
                jsonStr = jsonStr.Substring(3).Trim();
            if (jsonStr.EndsWith("```"))
                jsonStr = jsonStr.Substring(0, jsonStr.Length - 3).Trim();

            try
            {
                return JObject.Parse(jsonStr);
            }
            catch (JsonReaderException)
            {
                throw new Exception("Failed to parse valid JSON from response content");
            }
        }
    }
}