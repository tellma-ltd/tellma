using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Tellma.Services.Utilities
{
    /// <summary>
    /// Converts all DateTime values to the following format: "2021-02-15T01:17:13.286". <br/>
    /// Converts all DateTimeOffset values to the following format: "2021-02-15T01:17:13.2865330Z". <br/>
    /// </summary>
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string text;

            if (value is DateTime dateTime)
            {
                text = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", Culture);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                dateTimeOffset = dateTimeOffset.ToUniversalTime();
                text = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", Culture);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {value?.GetType()}.");
            }

            writer.WriteValue(text);
        }
    }
}
