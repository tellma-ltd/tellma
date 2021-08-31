using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellma.Api.Dto
{
    public static class JsonUtil
    {
        public static JsonSerializerOptions ConfigureOptionsForWeb(JsonSerializerOptions opt)
        {
            opt.Converters.Add(new ObjectConverter());
            opt.Converters.Add(new DateTimeConverter());
            opt.Converters.Add(new DateTimeOffsetConverter());

            opt.PropertyNamingPolicy = null;
            opt.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            return opt;
        }
    }
}
