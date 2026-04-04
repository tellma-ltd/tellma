using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class ExchangeRatesClient : CrudClientBase<ExchangeRateForSave, ExchangeRate, int>
    {
        internal ExchangeRatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "exchange-rates";

        public async Task<decimal> ConvertToFunctional(DateTime date, string currencyId, decimal amount, Request request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("convert-to-functional");
            urlBldr.AddQueryParameter("date", date.ToString("yyyy-MM-dd", ClientUtil.GregorianCulture));
            urlBldr.AddQueryParameter("currencyId", currencyId);
            urlBldr.AddQueryParameter("amount", amount.ToString());

            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            var result = await httpResponse.Content
                .ReadAsAsync<decimal>(cancellation)
                .ConfigureAwait(false);

            return result;
        }
    }
}
