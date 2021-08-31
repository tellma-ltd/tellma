using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Api.Dto
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public const string Format = "yyyy-MM-ddTHH:mm:ss.fff";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ToString(value));
        }

        /// <summary>
        /// Converts the <see cref="DateTimeOffset"/> value to the following format: "2021-02-15T01:17:13.2865330Z".
        /// </summary>
        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToString(Format);
        }
    }
}
