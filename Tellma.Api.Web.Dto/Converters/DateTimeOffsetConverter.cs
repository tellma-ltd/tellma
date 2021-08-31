using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Api.Dto
{
    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public const string Format = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ToString(value));
        }

        /// <summary>
        /// Converts the <see cref="DateTimeOffset"/>  value to the following format: "2021-02-15T01:17:13.286".
        /// </summary>
        public static string ToString(DateTimeOffset dateTimeOffset)
        {
            dateTimeOffset = dateTimeOffset.ToUniversalTime();
            return dateTimeOffset.ToString(Format);
        }
    }
}
