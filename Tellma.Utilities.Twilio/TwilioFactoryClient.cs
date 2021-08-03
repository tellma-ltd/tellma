using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Twilio.Clients;
using Twilio.Http;

namespace Tellma.Utilities.Twilio
{
    /// <summary>
    /// And implementation of <see cref="ITwilioRestClient"/> that benefits from the goodness of
    /// <see cref="System.Net.Http.IHttpClientFactory"/> as described here https://bit.ly/2EY1mcM.
    /// </summary>
    public class TwilioFactoryClient : ITwilioRestClient
    {
        private readonly TwilioRestClient _client;

        public TwilioFactoryClient(IOptions<TwilioOptions> options, System.Net.Http.HttpClient httpClient)
        {
            var opt = options.Value;

            _client = new TwilioRestClient(
                opt.AccountSid,
                opt.AuthToken,
                httpClient: new SystemNetHttpClient(httpClient));
        }


        public string AccountSid => _client.AccountSid;
        public string Region => _client.Region;
        public HttpClient HttpClient => _client.HttpClient;
        public Response Request(Request request) => _client.Request(request);
        public Task<Response> RequestAsync(Request request) => _client.RequestAsync(request);
    }
}
