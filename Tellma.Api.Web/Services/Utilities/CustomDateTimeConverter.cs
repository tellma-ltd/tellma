using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace Tellma.Services.Utilities
{
    /// <summary>
    /// Converts all <see cref="DateTime"/>  values to the following format: "2021-02-15T01:17:13.286". <br/>
    /// Converts all <see cref="DateTimeOffset"/>  values to the following format: "2021-02-15T01:17:13.2865330Z". <br/>
    /// </summary>
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string text;

            if (value is DateTime dateTime)
            {
                text = ToString(dateTime);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                text = ToString(dateTimeOffset);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {value?.GetType()}.");
            }

            writer.WriteValue(text);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/>  value to the following format: "2021-02-15T01:17:13.286".
        /// </summary>
        public static string ToString(DateTimeOffset dateTimeOffset)
        {
            dateTimeOffset = dateTimeOffset.ToUniversalTime();
            return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }


        /// <summary>
        /// Converts the <see cref="DateTimeOffset"/> value to the following format: "2021-02-15T01:17:13.2865330Z".
        /// </summary>
        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }
}
