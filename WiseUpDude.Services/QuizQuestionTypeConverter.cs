using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WiseUpDude.Model;

public class QuizQuestionTypeConverter : JsonConverter<QuizQuestionType>
{
    public override QuizQuestionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "TrueFalse" => QuizQuestionType.TrueFalse,
            "MultipleChoice" => QuizQuestionType.MultipleChoice,
            _ => throw new JsonException($"Unknown QuizQuestionType value: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, QuizQuestionType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            QuizQuestionType.TrueFalse => "TrueFalse",
            QuizQuestionType.MultipleChoice => "MultipleChoice",
            _ => throw new JsonException($"Unknown QuizQuestionType value: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}
