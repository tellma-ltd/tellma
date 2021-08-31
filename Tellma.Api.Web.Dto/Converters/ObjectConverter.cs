using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// If the target property is of type object, System.Text.Json does not attempt
    /// to infer its type of the deserialized JSON value. <br/>
    /// This converter implements the missing inference functionality.
    /// Further reading: https://bit.ly/3mNDi0c
    /// </summary>
    public class ObjectConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number => reader.GetDecimal(),
                JsonTokenType.String => DateTimeOrString(reader.GetString()),
                _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
            };
        }

        public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
        }

        /// <summary>
        /// Helper method that attempts to convert the input to a <see cref="DateTime"/> or 
        /// <see cref="DateTimeOffset"/> if it adheres to the precise formats used in the application.
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <returns>Either a <see cref="DateTime"/>, a <see cref="DateTimeOffset"/>, or a <see cref="string"/> depending on input.</returns>
        private static object DateTimeOrString(string input)
        {
            var provider = CultureInfo.InvariantCulture;
            var style = DateTimeStyles.None;

            if (DateTime.TryParseExact(input, DateTimeConverter.Format, provider, style, out DateTime t))
            {
                return t;
            }    
            else if (DateTimeOffset.TryParseExact(input, DateTimeOffsetConverter.Format, provider, style, out DateTimeOffset to))
            {
                return to;
            }
            else
            {
                return input;
            }
        }
    }
}
