using Tellma.Model.Application;

namespace Tellma.Client
{
    public class ExchangeRatesClient : CrudClientBase<ExchangeRateForSave, ExchangeRate, int>
    {
        internal ExchangeRatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "exchange-rates";
    }
}
