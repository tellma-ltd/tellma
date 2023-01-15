using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;

namespace Tellma.Client
{
    internal static class HttpContentExtensions
    {
        internal static async Task<T> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellation)
        {
            return await content.ReadFromJsonAsync<T>(
                options: _options,
                cancellationToken: cancellation);
        }

        private static readonly JsonSerializerOptions _options =
            JsonUtil.ConfigureOptionsForWeb(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
