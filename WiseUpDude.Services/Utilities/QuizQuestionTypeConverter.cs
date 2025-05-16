using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WiseUpDude.Model;

namespace WiseUpDude.Services.Utilities
{
    public class QuizQuestionTypeConverter : JsonConverter<QuizQuestionType>
    {
        public override QuizQuestionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            // Handle case-insensitivity and map unexpected values to a default
            return value?.ToLower() switch
            {
                "truefalse" => QuizQuestionType.TrueFalse,
                "multiplechoice" => QuizQuestionType.MultipleChoice,
                _ => throw new JsonException($"Invalid value for QuizQuestionType: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, QuizQuestionType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}