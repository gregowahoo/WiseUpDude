using System;
using System.Text;

public static class CleanJsonUtility
{
    public static string CleanJson(string input, out bool isLikelyTruncated)
    {
        isLikelyTruncated = false;
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Find the first '{' or '['
        int objStart = input.IndexOf('{');
        int arrStart = input.IndexOf('[');

        int start = -1;
        if (objStart >= 0 && arrStart >= 0)
            start = Math.Min(objStart, arrStart);
        else if (objStart >= 0)
            start = objStart;
        else if (arrStart >= 0)
            start = arrStart;

        if (start == -1)
            return string.Empty;

        string json = input.Substring(start).Trim();

        // Count braces/brackets to check for truncation
        int openCurly = 0, closeCurly = 0, openSquare = 0, closeSquare = 0, quoteCount = 0;
        bool inString = false;
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                inString = !inString;

            if (!inString)
            {
                if (c == '{') openCurly++;
                if (c == '}') closeCurly++;
                if (c == '[') openSquare++;
                if (c == ']') closeSquare++;
            }
        }

        // Check for unbalanced quotes or braces/brackets
        if (inString || openCurly > closeCurly || openSquare > closeSquare)
        {
            isLikelyTruncated = true;
            // Attempt to close braces/brackets if possible (not foolproof)
            var sb = new StringBuilder(json);
            if (inString) sb.Append('"');
            while (openCurly > closeCurly) { sb.Append('}'); closeCurly++; }
            while (openSquare > closeSquare) { sb.Append(']'); closeSquare++; }
            return sb.ToString();
        }

        return json;
    }
}